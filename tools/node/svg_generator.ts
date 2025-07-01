#!/usr/bin/env node

/**
 * SVG Generator Tool
 * 
 * Generates SVG elements based on input parameters.
 */

// Check if --info flag is used
if (process.argv.includes('--info')) {
  console.log(JSON.stringify({
    id: 'svg_generator',
    name: 'SVG Generator',
    description: 'Creates SVG images based on input parameters',
    version: '1.0.0',
    author: 'CognitiveMesh',
    inputs: {
      type: { 
        type: 'string', 
        description: 'Type of SVG to generate (circle, rect, icon)', 
        required: true 
      },
      size: { 
        type: 'number', 
        description: 'Size of the SVG in pixels', 
        default: 100 
      },
      color: { 
        type: 'string', 
        description: 'Main color of the SVG', 
        default: '#ff5500' 
      },
      text: { 
        type: 'string', 
        description: 'Text to include in SVG', 
        default: '' 
      }
    },
    outputs: {
      svg: 'string',
      width: 'number',
      height: 'number'
    }
  }));
  process.exit(0);
}

// Read input from stdin
let inputData = '';
process.stdin.on('data', (chunk) => {
  inputData += chunk;
});

process.stdin.on('end', () => {
  try {
    // Parse the input
    const input = JSON.parse(inputData);
    const { type, size = 100, color = '#ff5500', text = '' } = input;
    const context = input.context || {};
    
    let svg = '';
    let width = size;
    let height = size;
    
    // Generate SVG based on type
    switch (type) {
      case 'circle':
        svg = generateCircle(size, color, text);
        break;
        
      case 'rect':
        svg = generateRect(size, color, text);
        break;
        
      case 'icon':
        svg = generateIcon(size, color, text);
        break;
        
      default:
        throw new Error(`Unknown SVG type: ${type}`);
    }
    
    // Return the result
    console.log(JSON.stringify({
      svg,
      width,
      height,
      sessionId: context.sessionId,
      timestamp: new Date().toISOString()
    }));
    
    process.exit(0);
  } catch (error) {
    console.error(JSON.stringify({ error: error.message }));
    process.exit(1);
  }
});

// Helper functions to generate SVG elements
function generateCircle(size, color, text) {
  const radius = size / 2;
  const svg = `<svg width="${size}" height="${size}" xmlns="http://www.w3.org/2000/svg">
    <circle cx="${radius}" cy="${radius}" r="${radius - 5}" fill="${color}" />
    ${text ? `<text x="${radius}" y="${radius}" text-anchor="middle" dominant-baseline="middle" fill="white" font-family="Arial" font-size="${size/5}">${text}</text>` : ''}
  </svg>`;
  return svg;
}

function generateRect(size, color, text) {
  const svg = `<svg width="${size}" height="${size}" xmlns="http://www.w3.org/2000/svg">
    <rect width="${size - 10}" height="${size - 10}" x="5" y="5" fill="${color}" />
    ${text ? `<text x="${size/2}" y="${size/2}" text-anchor="middle" dominant-baseline="middle" fill="white" font-family="Arial" font-size="${size/5}">${text}</text>` : ''}
  </svg>`;
  return svg;
}

function generateIcon(size, color, text) {
  // Simple icon example
  const svg = `<svg width="${size}" height="${size}" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100">
    <path d="M30,10 L70,10 L90,50 L70,90 L30,90 L10,50 Z" fill="${color}" />
    ${text ? `<text x="50" y="55" text-anchor="middle" dominant-baseline="middle" fill="white" font-family="Arial" font-weight="bold" font-size="${size/5}">${text}</text>` : ''}
  </svg>`;
  return svg;
}