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

    chainWebpack: config =>
    {
        if (process.env.NODE_ENV === 'production')
        {
            config
                .plugin('uglify')
                .tap(args =>
                {
                    args[0].uglifyOptions.compress.keep_fnames = true;
                    args[0].uglifyOptions.mangle.keep_fnames = true;
                    return args;
                });
        }
    },
}