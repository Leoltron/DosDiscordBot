### v1.1.0 (20.01.2020)
 - Now current version and release date is shown in "Playing" status
 - Timeout increased to 5 minutes (was 1)
 - Added configurable rules: decks amount, penalties for callout and false callout, center row size, use images or text for displaying cards and first two house rules
 - Added **Center Row Penalty** house rule: at the end of your turn, draw as much cards as you left unmatched in the center row
 - Added **Draw Ends Turn** house rule: if you draw a card, your turn ends immediately

 #### v1.1.1 (23.01.2020)
 - Fixed counting match of two ones of different colors with Wild 2 as Double Color Match

 #### v1.1.2 (25.01.2020)
 - Added some clarifications in help command

 ### v1.2.0 (08.03.2020)
 - Now quitting does not stop the game as long as there's more that 1 player
 - Improved game logging
 - Added **Seven Swap** house rule: Color Match on 7 will force you to switch your hand  with somebody else
 - Added config option to change amount of cards players have to draw when somebody else made a Double Color Match 
 - Added bot players: right now they make random moves

 ### v1.2.1 (14.03.2020)
  - From this version, bot on Discord and repository is public
  - Added commands for inviting bot, support and source links
  
 ### v1.2.2 (07.04.2020)
  - Added game time to post game and table report 
 
 ### v1.2.3 (10.04.2020)
  - Now game can be stopped, rest of the players will be ranked by either hand score or hand size (depends on CardCountRanking config)
  
 ### v1.2.4 (23.04.2020)
  - Increased max amount of bot players to 2
  - Now bot uses players' nicknames instead of usernames
  - AI players don't have numbers added to their name anymore
  
 ### v1.2.5 (02.05.2020)
  - Elapsed time now has milliseconds
  - Fixed quitting messages
  
 ### v1.3.0 (29.06.2020)
  - Added **server default config**: when you use `dos config` in channel where is no game created, it prints or updates server default config. Values in it will be copied to every new game on that server. You need «Manage Roles» permission to edit server configuration.
  
 ### v1.4.0 (03.07.2020)
  - Switched to embeds for displaying center row
  
 ### v1.5.0 (07.11.2020)
  - Added command for hybrid card displaying: both text and images

 ### v1.6.0 (22.02.2021)
  - Added replay saving feature
  - Made AI a little less random and a little smarter