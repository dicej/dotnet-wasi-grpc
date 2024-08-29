import * as http from 'node:http';

const server = http.createServer((req, res) => {
    if (req.method === "POST" && req.url === "/echo") {
        // Note that we buffer the request body here rather than pipe it
        // directly to the response.  That's because, as of this writing,
        // `WasiHttpHandler` sends the entire request body before reading
        // any of the response body, which can lead to deadlock if the
        // server is blocked on backpressure when sending the response body.
        let chunks = [];
        req.on("data", (chunk) => {
            chunks.push(chunk);
        });
        req.on("end", () => {
            res.writeHead(200, req.headers);
            res.end(Buffer.concat(chunks));
        });
    } else if (req.method === "GET" && req.url === "/slow-hello") {
        setTimeout(() => {
            const body = "hola";
            res
                .writeHead(200, {
                    "content-length": Buffer.byteLength(body),
                    "content-type": "text/plain",
                })
                .end(body);
        }, 10 * 1000);
    } else {
        let status;
        let body;
        if (req.method === "GET" && req.url === "/hello") {
            status = 200;
            body = "hola";
        } else {
            status = 400;
            body = "Bad Request";
        }
        res
            .writeHead(status, {
                "content-length": Buffer.byteLength(body),
                "content-type": "text/plain",
            })
            .end(body);
    }
});
server.listen(() => {
    console.log("listening on", server.address().port);
});
