# chess.com.downloader
Download games in pgn format for a [chess.com](http://www.chess.com) user
download is very fast using multiple requests at once, leveraging asynchronous I/O, but fair enought to not flood the chess.com api.
[Here you can find chess.com api reference](https://www.chess.com/news/view/published-data-api)

I verified the quality of the file by importing in [SCID](http://scid.sourceforge.net/) and it worked just fine.
I run it on a Linux machine, and I used asynchronous I/O even during file writing. Not sure this can cause problems if we have some linitations on maximum number of open files, it could be since many file are opened at once while download massive databases. 
