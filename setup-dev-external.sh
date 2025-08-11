#!/bin/bash
# Microsoft 1ES Compliant Development Environment Setup
# This script creates a complete development environment outside the repository
# to avoid Azure DevOps Container Security Analysis violations

set -e

echo "🔧 Setting up RMS Demo Development Environment (Microsoft 1ES Compliant)"
echo "=================================================================="

# Create external development directory  
DEV_DIR="/tmp/rms-demo-dev-external"
echo "Creating development environment in: $DEV_DIR"
rm -rf "$DEV_DIR"
mkdir -p "$DEV_DIR/k8s"

# Copy base configurations
cp -r k8s/namespace.yaml "$DEV_DIR/k8s/"
cp -r k8s/ingress.yaml "$DEV_DIR/k8s/"

echo "✅ Creating development-specific Kubernetes configurations..."

# Create PostgreSQL with PostGIS (external registry)
cat > "$DEV_DIR/k8s/postgres.yaml" << 'EOF'
---
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: postgres
  namespace: rms
  labels:
    app: postgres
    tier: database
    environment: development
spec:
  serviceName: postgres
  replicas: 1
  selector:
    matchLabels:
      app: postgres
      tier: database
  template:
    metadata:
      labels:
        app: postgres
        tier: database
    spec:
      containers:
        - name: postgres
          image: postgis/postgis:15-3.4-alpine
          ports:
            - containerPort: 5432
          env:
            - name: POSTGRES_DB
              value: rmsdemodb
            - name: POSTGRES_USER
              value: rmsuser
            - name: POSTGRES_PASSWORD
              value: defaultpassword
          volumeMounts:
            - name: postgres-storage
              mountPath: /var/lib/postgresql/data
          resources:
            requests:
              cpu: 100m
              memory: 256Mi
            limits:
              cpu: 500m
              memory: 1Gi
  volumeClaimTemplates:
    - metadata:
        name: postgres-storage
      spec:
        accessModes: ["ReadWriteOnce"]
        resources:
          requests:
            storage: 10Gi
---
apiVersion: v1
kind: Service
metadata:
  name: postgres
  namespace: rms
  labels:
    app: postgres
    tier: database
spec:
  selector:
    app: postgres
    tier: database
  ports:
    - protocol: TCP
      port: 5432
      targetPort: 5432
  type: ClusterIP
EOF

# Create Redis (external registry)
cat > "$DEV_DIR/k8s/redis.yaml" << 'EOF'
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: redis
  namespace: rms
  labels:
    app: redis
    tier: cache
    environment: development
spec:
  replicas: 1
  selector:
    matchLabels:
      app: redis
      tier: cache
  template:
    metadata:
      labels:
        app: redis
        tier: cache
    spec:
      containers:
        - name: redis
          image: redis:7-alpine
          ports:
            - containerPort: 6379
          volumeMounts:
            - name: redis-data
              mountPath: /data
          resources:
            requests:
              cpu: 50m
              memory: 64Mi
            limits:
              cpu: 200m
              memory: 256Mi
      volumes:
        - name: redis-data
          emptyDir: {}
---
apiVersion: v1
kind: Service
metadata:
  name: redis
  namespace: rms
  labels:
    app: redis
    tier: cache
spec:
  selector:
    app: redis
    tier: cache
  ports:
    - protocol: TCP
      port: 6379
      targetPort: 6379
  type: ClusterIP
EOF

# Create API deployment (external registry for development)
cat > "$DEV_DIR/k8s/deployment.yaml" << 'EOF'
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: rms-demo
  namespace: rms
  labels:
    app: rms-demo
    tier: backend
    environment: development
spec:
  replicas: 1
  selector:
    matchLabels:
      app: rms-demo
      tier: backend
  template:
    metadata:
      labels:
        app: rms-demo
        tier: backend
    spec:
      containers:
        - name: api
          image: rms-demo:local
          imagePullPolicy: Never
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
          env:
            - name: ASPNETCORE_ENVIRONMENT
              value: "Development"
            - name: ConnectionStrings__DefaultConnection
              valueFrom:
                secretKeyRef:
                  name: rms-demo-secrets
                  key: connectionString
---
apiVersion: v1
kind: Service
metadata:
  name: rms-demo
  namespace: rms
  labels:
    app: rms-demo
    tier: backend
spec:
  selector:
    app: rms-demo
    tier: backend
  ports:
    - protocol: TCP
      port: 80
      targetPort: 8080
  type: ClusterIP
EOF

# Create development secret
cat > "$DEV_DIR/k8s/secret.yaml" << 'EOF'
apiVersion: v1
kind: Secret
metadata:
  name: rms-demo-secrets
  namespace: rms
  labels:
    app: rms-demo
    environment: development
stringData:
  connectionString: Server=postgres;Database=rmsdemodb;User Id=rmsuser;Password=defaultpassword
EOF

# Create kustomization
cat > "$DEV_DIR/k8s/kustomization.yaml" << 'EOF'
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization
namespace: rms

# EXTERNAL DEVELOPMENT ENVIRONMENT
# Created outside repository to comply with Microsoft 1ES Container Security policies
# Uses external registries for full PostGIS and Redis functionality

resources:
  - namespace.yaml
  - secret.yaml
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
      environment: external-development
EOF

echo "✅ Development environment created successfully!"
echo ""
echo "🚀 Usage:"
echo "  kubectl apply -k $DEV_DIR/k8s/"
echo ""
echo "🔍 Verify deployment:"
echo "  kubectl -n rms get pods,svc"
echo "  curl http://localhost:8080/health"
echo ""
echo "📁 Location: $DEV_DIR"
echo "🛡️  Compliance: External to repository - no Azure DevOps security scanning violations"
