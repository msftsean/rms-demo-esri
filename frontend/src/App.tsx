import React, { useEffect, useState } from 'react'
import MapView from './components/MapView'
import axios from 'axios'

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080'

export default function App() {
  const [records, setRecords] = useState<any[]>([])

  useEffect(() => {
    axios.get(`${API_BASE}/api/records`)
      .then(r => setRecords(r.data))
      .catch(() => setRecords([]))
  }, [])

  return (
    <div className="app">
      <header className="rainbow-header">
        <div className="container header-inner">
          <div className="brand">
            <svg width="28" height="28" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true"><path d="M8 0C3.58 0 0 3.58 0 8a8 8 0 0 0 5.47 7.59c.4.07.55-.17.55-.38..."/></svg>
            <span>RMS Demo ESRI</span>
          </div>
          <nav className="nav">
            <a href="#">Home</a>
            <a href="#">Records</a>
            <a href="https://github.com/msftsean/rms-demo-esri" target="_blank" rel="noreferrer">GitHub</a>
          </nav>
        </div>
      </header>

      <main className="container main-grid">
        <section className="panel">
          <h2>Interactive Map</h2>
          <MapView />
        </section>

        <section className="panel">
          <h2>Recent Records</h2>
          {records.length === 0 && <div className="empty">No records yet. Create one via API.</div>}
          <ul className="records">
            {records.map(r => (
              <li key={r.id}>
                <strong>{r.title}</strong>
                <div className="muted">{r.description}</div>
              </li>
            ))}
          </ul>
        </section>
      </main>

      <footer className="footer container">
        Built with ❤️ • Rainbow theme • GitHub-inspired
      </footer>
    </div>
  )
}
