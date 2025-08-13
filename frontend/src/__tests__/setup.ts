import '@testing-library/jest-dom';

// Mock ResizeObserver for ArcGIS Core
global.ResizeObserver = class ResizeObserver {
  observe() {}
  unobserve() {}
  disconnect() {}
};
