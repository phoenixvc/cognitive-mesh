import type { Meta, StoryObj } from '@storybook/react';
import { DesignSystemShowcase } from './DesignSystemShowcase';

const meta: Meta<typeof DesignSystemShowcase> = {
  title: 'Design System/DesignSystemShowcase',
  component: DesignSystemShowcase,
  parameters: {
    layout: 'fullscreen',
    docs: {
      description: {
        component: 'A comprehensive showcase of the Cognitive Mesh design system, including all design tokens, typography, colors, spacing, and components.',
      },
    },
  },
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  args: {},
};

export const FullShowcase: Story = {
  args: {},
  parameters: {
    docs: {
      description: {
        story: 'Complete design system showcase with all tokens and components displayed together.',
      },
    },
  },
}; 