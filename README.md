# Balckout Monitor

## Beeper machine script
```Bash
curl --location --include --request POST "https://asrv-blackout-monitor.azurewebsites.net/beepers/d13/status" \
--header "Content-Type: application/json" \
--data-raw "{\"status\": \"Healthy\"}"
```