# PHC-HighCounsel-Bot

Bot for the guys in Discord.

## Setup for Ollama (Linux)

Use installer like normal then continue here...

### Setting environment variables on Linux

If Ollama is run as a systemd service, environment variables should be set using `systemctl`:

1. Edit the systemd service by calling `systemctl edit ollama.service`. This will open an editor.

2. For each environment variable, add a line `Environment` under section `[Service]`:

    ```ini
    [Service]
    Environment="OLLAMA_HOST=0.0.0.0"
    ```

3. Save and exit.

4. Reload `systemd` and restart Ollama:

   ```bash
   systemctl daemon-reload
   systemctl restart ollama
   ```
