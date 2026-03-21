import type { StorybookConfig } from "@storybook/react-webpack5";

const config: StorybookConfig = {
  stories: [
    "../src/**/*.mdx",
    "../src/**/*.stories.@(js|jsx|mjs|ts|tsx)",
  ],
  addons: [
    "@storybook/addon-links",
  ],
  framework: {
    name: "@storybook/react-webpack5",
    options: {},
  },
  docs: {},
  webpackFinal: async (config) => {
    // Remove custom rules for .css and .ts(x) files
    // Add Babel loader for TypeScript/TSX
    config.module?.rules?.push({
      test: /\.(ts|tsx)$/,
      exclude: /node_modules/,
      use: [
        {
          loader: 'babel-loader',
          options: {
            presets: [
              '@babel/preset-env',
              '@babel/preset-react',
              '@babel/preset-typescript'
            ]
          }
        }
      ]
    });
    return config;
  },
};

export default config; 