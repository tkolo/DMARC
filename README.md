# DMARC report aggregator

DMARC report aggregator connects to your mailbox through IMAP protocol
and then parses and stores all reports in elasticsearch database so that they can be browsed using tools like kibana.
It provides simple interface to browse the reports as well the ability to send selected reports
to your mailbox in a more human friendly format.

## Major features include

- ability to connect to mailbox using IMAP protocol and then using IMAP IDLE command to monitor it
- listen to multiple inboxes at once
- send received reports in nice HTML formatted email to selected users, 
filterable by severity of report (send all, partial failures or only failures)
- WebUI to configure the IMAP clients

## Getting started

First, you need a working elasticsearch server to connect to.
Just set correct url in appsettings.json and you're good to go.
Since there're no builds provided at the time of writing this README,
you either need to build and publish the project yourself or have dotnet core SDK installed on the host.

## Configuring development environment

### Requirements
- .NET Core 2.1 or higher
- elasticsearch server instance

### Setup

- Make sure you have the software above
- Clone the repo
- That's it!

### License
This software is licensed under GNU GPLv3
