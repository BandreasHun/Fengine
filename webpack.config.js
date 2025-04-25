const path = require("path");

module.exports = {
  entry: "./src/entry.fs.js",
  output: {
    path: path.join(__dirname, "./dist"),
    filename: "bundle.js"
  },
  mode: "development",
  devServer: {
    static: {
      directory: path.join(__dirname, "public")
    },
    port: 8080
  },
  module: {
    rules: [
      {
        test: /\.fs(x|proj)?$/,
        use: "fable-loader"
      }
    ]
  },
  resolve: {
    modules: [path.resolve(__dirname, "node_modules")]
  }
};
