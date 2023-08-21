# Database connection example

This repository contains the code for
the [database connection example](https://github.com/MrOkiDoki/BattleBit-Community-Server-API/wiki/Database-with-stats)
for the [BattleBit Community API](https://github.com/MrOkiDoki/BattleBit-Community-Server-API).

Features:
- !banweapon & !unbanweapon commands that save to DB, where it checks the DB on every spawn request to see if a weapon is allowed.
- Gameserver whitelist in DB that stores the token for every IP+port combination.
- Saving and pulling player stats / progression to and from DB.