import React, { useEffect, useRef } from 'react'
import Map from '@arcgis/core/Map'
import MapView from '@arcgis/core/views/MapView'
import esriConfig from '@arcgis/core/config'
import '@arcgis/core/assets/esri/themes/light/main.css'

const API_KEY = import.meta.env.VITE_ARCGIS_API_KEY ?? ''

export default function MapViewComponent() {
  const divRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    esriConfig.apiKey = API_KEY

    const map = new Map({ basemap: 'arcgis-topographic' })
    const view = new MapView({
      map,
      container: divRef.current!,
      center: [-98.5795, 39.8283],
      zoom: 4,
      constraints: { snapToZoom: false }
    })

    return () => view.destroy()
  }, [])

  return <div ref={divRef} className="esri-view" />
}
