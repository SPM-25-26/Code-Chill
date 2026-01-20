# Chat Prompt Template (Deterministic)

The chatbot prompt is built with fixed sections in this order. Empty sections are represented by `-`.

1) SYSTEM
2) USER_PROFILE
3) LOCATION
4) CONVERSATION
5) USER_MESSAGE
6) TOURISM_DATA

Each tourism data line is formatted as:

`- [type] name | id=... | category=... | municipality=... | address=... | distance_km=... | availability=... | notes=...`

Notes:
- Only ingested tourism data is included.
- If no data is available for a field, the value is `-`.
- The conversation list is capped to the last 10 messages.
