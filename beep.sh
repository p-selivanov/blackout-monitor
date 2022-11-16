#!/bin/bash

base_uri="https://asrv-blackout-monitor.azurewebsites.net/"
beeper_id="d13"
uri="${base_uri}beepers/${beeper_id}/status"
body="{\"status\":\"Healthy\"}"

status_code=$(curl --request POST $uri \
--header "Content-Type: application/json" \
--data-raw $body \
--write-out "%{http_code}" --silent --show-error --output /dev/null)

time=$(date "+%H:%M:%S")
echo "[$time] responded $status_code"