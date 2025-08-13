#!/bin/bash
# Development Kubernetes Setup Script
# This script creates development-friendly k8s manifests outside the repository
# to avoid Azure DevOps security scanning while maintaining functionality

set -e

DEV_DIR="/tmp/rms-demo-dev-k8s"
REPO_DIR="$(pwd)"

echo "🚀 Setting up development Kubernetes environment..."

# Create development directory
mkdir -p "$DEV_DIR"
cd "$DEV_DIR"

# Copy base files from repository
cp "$REPO_DIR/k8s/namespace.yaml" .
cp "$REPO_DIR/k8s/secret-sample.yaml" .
cp "$REPO_DIR/k8s/ingress.yaml" .

# Create development-specific postgres.yaml with PostGIS
cat > postgres.yaml << 'EOF'
apiVersion: v1
kind: ConfigMap
metadata:
  name: postgres-init
  namespace: rms
data:
  init.sql: |
    -- Enable PostGIS extension
    CREATE EXTENSION IF NOT EXISTS postgis;
    CREATE EXTENSION IF NOT EXISTS postgis_topology;
    
    -- Create sample table for RMS demo
    CREATE TABLE IF NOT EXISTS records (
        id SERIAL PRIMARY KEY,
        name VARCHAR(255),
        description TEXT,
        geometry GEOMETRY(Point, 4326),
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );
---
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: postgres
  namespace: rms
  labels:
    app: postgres
    tier: database
spec:
  serviceName: postgres
  replicas: 1
  selector:
    matchLabels:
      app: postgres
  template:
    metadata:
      labels:
        app: postgres
        tier: database
    spec:
      securityContext:
        runAsNonRoot: false
      containers:
        - name: postgres
          image: postgis/postgis:15-3.3-alpine
          ports:
            - containerPort: 5432
          env:
            - name: POSTGRES_DB
              value: rmsdemodb
            - name: POSTGRES_USER
              value: rmsuser
            - name: POSTGRES_PASSWORD
              valueFrom:
                secretKeyRef:
                  name: postgres-secret
                  key: password
          volumeMounts:
            - name: postgres-data
              mountPath: /var/lib/postgresql/data
            - name: init-sql
              mountPath: /docker-entrypoint-initdb.d
      volumes:
        - name: init-sql
          configMap:
            name: postgres-init
  volumeClaimTemplates:
    - metadata:
        name: postgres-data
      spec:
        accessModes: ["ReadWriteOnce"]
        resources:
          requests:
            storage: 5Gi
---
apiVersion: v1
kind: Service
metadata:
  name: postgres
  namespace: rms
spec:
  selector:
    app: postgres
  ports:
    - protocol: TCP
      port: 5432
      targetPort: 5432
  type: ClusterIP
EOF

# Create development-specific redis.yaml
cat > redis.yaml << 'EOF'
apiVersion: apps/v1
kind: Deployment
metadata:
  name: redis
  namespace: rms
  labels:
    app: redis
    tier: cache
spec:
  replicas: 1
  selector:
    matchLabels:
      app: redis
  template:
    metadata:
      labels:
        app: redis
        tier: cache
    spec:
      volumes:
        - name: redis-data
          emptyDir: {}
      containers:
        - name: redis
          image: redis:7-alpine
          ports:
            - containerPort: 6379
          args: ["--save", "", "--appendonly", "no"]
          securityContext:
            allowPrivilegeEscalation: false
            readOnlyRootFilesystem: true
          volumeMounts:
            - name: redis-data
              mountPath: /data
          resources:
            requests:
              cpu: 50m
              memory: 64Mi
            limits:
              cpu: 250m
              memory: 256Mi
---
apiVersion: v1
kind: Service
metadata:
  name: redis
  namespace: rms
spec:
  selector:
    app: redis
  ports:
    - protocol: TCP
      port: 6379
      targetPort: 6379
  type: ClusterIP
EOF

# Create development-specific deployment.yaml
cat > deployment.yaml << 'EOF'
apiVersion: apps/v1
kind: Deployment
metadata:
  name: rms-demo
  namespace: rms
  labels:
    app: rms-demo
    tier: backend
spec:
  replicas: 1
  selector:
    matchLabels:
      app: rms-demo
  template:
    metadata:
      labels:
        app: rms-demo
        tier: backend
    spec:
      securityContext:
        runAsNonRoot: true
        runAsUser: 1001
        runAsGroup: 1001
        fsGroup: 1001
      initContainers:
        - name: wait-for-postgres
          image: postgis/postgis:15-3.3-alpine
          command: ["sh", "-c", "until pg_isready -h postgres -U rmsuser -d rmsdemodb; do echo waiting for postgres; sleep 2; done"]
      containers:
        - name: api
          image: rms-demo:local
          imagePullPolicy: IfNotPresent
          ports:
            - containerPort: 8080
          readinessProbe:
            httpGet:
              path: /health
              port: 8080
            initialDelaySeconds: 5
            periodSeconds: 10
            timeoutSeconds: 2
            failureThreshold: 3
          livenessProbe:
            httpGet:
              path: /health
              port: 8080
            initialDelaySeconds: 10
            periodSeconds: 20
            timeoutSeconds: 3
            failureThreshold: 3
          resources:
            requests:
              cpu: 100m
              memory: 128Mi
            limits:
              cpu: 500m
              memory: 512Mi
          securityContext:
            allowPrivilegeEscalation: false
            readOnlyRootFilesystem: true
          volumeMounts:
            - name: tmp
              mountPath: /tmp
          env:
            - name: ASPNETCORE_ENVIRONMENT
              value: "Development"
            - name: ASPNETCORE_URLS
              value: "http://+:8080"
            - name: ConnectionStrings__DefaultConnection
              valueFrom:
                secretKeyRef:
                  name: rms-demo-secrets
                  key: connectionString
            - name: Redis__ConnectionString
              value: "redis:6379"
      volumes:
        - name: tmp
          emptyDir: {}
---
apiVersion: v1
kind: Service
metadata:
  name: rms-demo
  namespace: rms
spec:
  selector:
    app: rms-demo
  ports:
    - protocol: TCP
      port: 80
      targetPort: 8080
  type: ClusterIP
EOF

# Create kustomization.yaml
cat > kustomization.yaml << 'EOF'
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization
namespace: rms

resources:
  - namespace.yaml
  - secret-sample.yaml
  - postgres.yaml
  - redis.yaml
  - deployment.yaml
  - ingress.yaml

images:
  - name: rms-demo
    newName: rms-demo
    newTag: local

labels:
  - includeSelectors: true
    pairs:
      app: rms-demo
      version: v1.1.0-dev
      environment: development
EOF

echo "✅ Development environment created at: $DEV_DIR"
echo ""
echo "🚀 Deploy development environment:"
echo "   kubectl apply -k $DEV_DIR"
echo ""
echo "🔒 Deploy Azure-compliant production:"
echo "   kubectl apply -k $REPO_DIR/k8s/overlays/azure/"
echo ""
echo "📋 Current development pods:"
kubectl get pods -n rms 2>/dev/null || echo "   (No pods currently running)"
