import React from 'react';
import { render, screen } from '@testing-library/react';
import { CognitiveMeshButton } from './CognitiveMeshButton';

describe('CognitiveMeshButton', () => {
  it('renders the button with the correct text', () => {
    render(<CognitiveMeshButton>Click me</CognitiveMeshButton>);
    const buttonElement = screen.getByText(/Click me/i);
    expect(buttonElement).toBeInTheDocument();
  });
});
