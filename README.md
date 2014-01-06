ChunkedStreamTest
=================
This solution allows to reproduce my chunked stream reponse problem reported on http://forums.asp.net/p/1957864/5596454.aspx?Re+PushStreamContent+client+makes+two+requests+

Compile and run first the ChunkedStreamServer, then the ChnkedStreamClient.

The Client should make one single request to http://localhost:8091/PollData, get a chunked response, and dump whatever data it gets.

The server dumps a rest string {"data":"number"} where number counts up every second.

As is, you can see the server's PollData method being hit once, for the client request, and then sending data to two Streams, but the client gets every message only once.
