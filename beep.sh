#!/bin/bash

status_code=$(curl --location --request POST "https://asrv-blackout-monitor.azurewebsites.net/beepers/d13/status" \
--header "Content-Type: application/json" \
--data-raw "{\"status\": \"Healthy\"}" \
--write-out "%{http_code}" --silent)

time=$(date "+%H:%M:%S")
echo "[$time] responded $status_code"