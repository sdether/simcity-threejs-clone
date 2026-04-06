export default {
  // Set the base directory for GitHub pages
  base: '/simcity-threejs-clone/',

  // Set the project root directory (relative to the config file)
  root: './src',

  // Set the directory to serve static files from (relative to the root)
  publicDir: './public',

  // Set the build output directory
  build: {
    outDir: './dist'
  },

  server: {
    host: '127.0.0.1',
    port: 3333,
    proxy: {
      '/api': {
        target: 'http://127.0.0.1:3001',
        changeOrigin: true,
      },
      '/ws': {
        target: 'ws://127.0.0.1:3001',
        ws: true,
      },
    },
  },
}