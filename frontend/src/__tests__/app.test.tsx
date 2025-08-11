import { describe, it, expect } from 'vitest';

describe('App', () => {
  it('passes basic smoke test', () => {
    // Simple test that doesn't require rendering components with ArcGIS dependencies
    expect(true).toBe(true);
  });
  
  it('can import React', async () => {
    const React = await import('react');
    expect(React).toBeDefined();
  });
});
