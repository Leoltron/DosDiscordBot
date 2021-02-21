const path = require("path");
module.exports = {
    entry: {
        replay: "./front/src/js/entry/replay.js"
    },
    output: {
        path: path.resolve(__dirname, "front/dist/js"),
        filename: "[name].js"
    },
    module: {
        rules: [
            {
                test: /\.js$/,
                exclude: /(node_modules|bower_components)/,
                use: {
                    loader: 'babel-loader',
                    options: {
                        presets: ["@babel/preset-env"]
                    }
                }
            }
        ]
    },
};