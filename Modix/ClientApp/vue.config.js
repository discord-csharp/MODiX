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

    configureWebpack: config =>
    {
        if (process.env.NODE_ENV === 'production')
        {
            config.optimization.minimizer[0].options.uglifyOptions.compress.keep_fnames = true;
            config.optimization.minimizer[0].options.uglifyOptions.mangle.keep_fnames = true;
        }
    },

    chainWebpack: config =>
    {
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
    }
}