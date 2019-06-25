const http = require('http');

//Check if the dev server is running by making a web request
//to localhost:8080. If you change your dev server port, change this
//too. Hardcoding sucks but it's better than build collisions.

http.get('http://localhost:8080', (resp) => {
    //If it's up, don't do anything - error code 0
})
.on("error", (err) => {
    return process.exit(1);
});