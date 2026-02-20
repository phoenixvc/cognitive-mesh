module.exports = {
  source: ['tokens/**/*.json'],
  platforms: {
    css: {
      transforms: ['attribute/cti', 'name/cti/kebab', 'size/px', 'color/hex'],
      buildPath: 'build/css/',
      files: [{
        destination: 'cognitive-mesh-tokens.css',
        format: 'css/variables',
        options: {
          outputReferences: true
        }
      }]
    },
    scss: {
      transforms: ['attribute/cti', 'name/cti/kebab', 'size/px', 'color/hex'],
      buildPath: 'build/scss/',
      files: [{
        destination: '_cognitive-mesh-tokens.scss',
        format: 'scss/variables',
        options: {
          outputReferences: true
        }
      }]
    }
  }
}; 