# ECET230FinalMQTTViewer

#Serial Protocall

\#\#\#0000\#\#[ASCII Screen Serialized Data Payload]\#\#0000

Header,four digit payload length, data payload, four digit checksum

If packet with data payload length of 0 is sent to the meadow, the meadow will respond with its saved screen data.