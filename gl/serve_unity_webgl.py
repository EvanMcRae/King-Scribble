# serve_unity_webgl.py
import http.server
import socketserver
import os

# Change this to the folder where your Unity WebGL build is located
WEBGL_BUILD_FOLDER = "." 

PORT = 8000

# This is our custom handler
class BrotliHTTPRequestHandler(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        # Check if the file being sent is compressed
        path = self.translate_path(self.path)
        if path.endswith(".br"):
            self.send_header("Content-Encoding", "br")
        elif path.endswith(".gz"):
            self.send_header("Content-Encoding", "gzip")
        
        # Also set the correct MIME types for WebGL files
        if path.endswith(".wasm.br") or path.endswith(".wasm.gz") or path.endswith(".wasm"):
            self.send_header("Content-Type", "application/wasm")
        elif path.endswith(".js.br") or path.endswith(".js.gz") or path.endswith(".js"):
            self.send_header("Content-Type", "application/javascript")

        # Call the parent class's end_headers method
        super().end_headers()

# Change working directory to the Unity WebGL build folder
try:
    os.chdir(WEBGL_BUILD_FOLDER)
except FileNotFoundError:
    print(f"Error: The folder '{WEBGL_BUILD_FOLDER}' was not found.")
    print("Please make sure this script is in the parent directory of your build folder, or update the WEBGL_BUILD_FOLDER variable.")
    exit()


# Use our custom handler instead of the default one
Handler = BrotliHTTPRequestHandler

with socketserver.TCPServer(("", PORT), Handler) as httpd:
    print(f"Serving Unity WebGL build at http://localhost:{PORT}")
    print("Press Ctrl+C to stop.")
    httpd.serve_forever()