# Balckout Monitor
Server application for monitoring blackouts.
Monitors a few signaling devices, and figures out when a device is offline.
Writes history of each blackout to the Cosmos DB and notifies a Telegram channel about it.

This solution uses the "Push Model". Which means that each device should push to the server it's health status.
Such device is calles a Beeper and one signal is a Beep.
The server has a REST API to receive health status calls from the Beepers.
If server does not receive a Beep for some time (75s be default) - it considers this Beeper unhealthy and creates a Blackout.

## Beeper Setup
At this point the solution is tested only with Raspberry Pi on linux as a Beeper.

Setup:
1. Install Raspberry Pi Lite and connect the device to the Internet.
2. Add the beep script (copy `beep.sh`).
3. Update the script with correct server URI and Beeper ID.
4. Create a CRON job to run the script each minute (`* * * * * /home/paul/beeper/beep.sh >> /home/paul/beeper/log 2>&1`);

## TODO
- Handle too often switches
- Review manual & timer switches
- Review restore scenarios