// Learn more: https://github.com/testing-library/jest-dom
require('@testing-library/jest-dom');

// Polyfill crypto.randomUUID for jsdom
if (typeof globalThis.crypto === 'undefined') {
  globalThis.crypto = {};
}
if (typeof globalThis.crypto.randomUUID !== 'function') {
  let counter = 0;
  globalThis.crypto.randomUUID = () => {
    counter++;
    return `00000000-0000-4000-8000-${String(counter).padStart(12, '0')}`;
  };
}

// Polyfill TextEncoder/TextDecoder for jsdom
const { TextEncoder, TextDecoder } = require('util');
if (typeof globalThis.TextEncoder === 'undefined') {
  globalThis.TextEncoder = TextEncoder;
}
if (typeof globalThis.TextDecoder === 'undefined') {
  globalThis.TextDecoder = TextDecoder;
}
