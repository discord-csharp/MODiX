const ForkTsCheckerWebpackPlugin = require('fork-ts-checker-webpack-plugin');
const webpack = require('webpack');

module.exports = {
    "devServer":
    {
        "proxy":
        {
            "/api":
            {
                "target": "http://localhost:5000",
                "changeOrigin": true
            }
        }
    },

    css:
    {
        extract: false
    },

    configureWebpack:
    {
        devtool: 'source-map'
    },

    chainWebpack: config =>
    {
        config.resolve

        config
            .plugin('html')
            .tap(args =>
            {
				if (args[0] && args[0].minify)
				{
					args[0].minify.removeScriptTypeAttributes = false;
				}

                return args;
            });

        config
            .plugin('fork-ts-checker')
                .use(ForkTsCheckerWebpackPlugin,
                    [{
                        checkSyntacticErrors: true,
                        vue: true,
                        formatter: 'codeframe',
                        workers: ForkTsCheckerWebpackPlugin.TWO_CPUS_FREE
                    }]);

        config.module
            .rule('ts')
            .use('ts-loader')
                .loader('ts-loader')
                .tap(args =>
                {
                    args.experimentalWatchApi = true;
                    return args;
                });

        config.resolve.alias
            .set('chart.js', 'chart.js/dist/Chart.js');

        config
            .plugin('context-replacement')
                .use(webpack.IgnorePlugin, [/^\.\/locale$/, /moment$/]);
    }
}