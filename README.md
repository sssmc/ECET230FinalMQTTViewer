# ECET230FinalMQTTViewer

# Serial Protocall

##[Packet Type]0000[Payload]0000

Header,one digit packet type,four digit payload length, data payload, four digit checksum(

## Example

##01234{"Connection":{"MQTTClientId":"FDkPCxA2KTkHMgANKik6NgI","MQTTHost":"broker.mqtt-dashboard.com","MQTTPort":1883,"MQTTUsername":"FDkPCxA2KTkHMgANKik6NgI","MQTTPassword":"lRBFHoyhV9ruKuh0sy7s0QXm","WifiSSID":"White Rabbit","WifiPassword":"2511560A7196"},"Indicators":[[{"Name":"Temperature","Topic":"channels/2328115/subscribe/fields/field1"},{"Name":"Humidity","Topic":"channels/232811/subscribe/fields/field2"}],[{"Name":"Random1","Topic":"channels/2328115/subscribe/fields/field3"},{"Name":"Random2","Topic":"channels/2328115/subscribe/fields/field4"},{"Name":"Button","Topic":"ECET260/Sebastien/button"},{"Name":"Humidity","Topic":"channels/2328115/subscribe/fields/field2"}]]}1234

## Packet Types
0: Screen Data - packet containing serialized ScreenData class as payload
1: Request Data - packet with no payload sent to request Screen Data packet from the receving device
