var path = require('path');
var webpack = require('webpack');
var fableUtils = require('fable-utils');

function resolve(filePath) {
  return path.join(__dirname, filePath)
}

var isProduction = process.argv.indexOf("-p") >= 0;
console.log("Bundling for " + (isProduction ? "production" : "development") + "...");

var babelOptions = fableUtils.resolveBabelOptions({
  presets: [["es2015", { "modules": false }]],
  plugins: ["transform-runtime"]
});

module.exports = {
  devtool: 'source-map',
  entry: resolve('src/Tests.Frontend/Tests.Frontend.fsproj'),
  output: {
    filename: 'disco.tests.js',
    path: resolve('js'),
  },
  resolve: {
    extensions: ['.js', '.json'],
    modules: [
      "node_modules", resolve("../../node_modules/")
    ]
  },
  module: {
    rules: [
      {
        test: /\.fs(x|proj)?$/,
        use: {
          loader: 'fable-loader',
          options: {
            babel: babelOptions,
            define: isProduction ? [] : ["DEBUG"],
            plugins: resolve("src/FlatBuffersPlugin/bin/Release/netstandard2.0/FlatBuffersPlugin.dll"),
          }
        }
      },
      {
        test: /\.js$/,
        exclude: /node_modules[\\\/](?!fable-)/,
        use: {
          loader: 'babel-loader',
          options: babelOptions
        },
      },
    ],
  }
};
