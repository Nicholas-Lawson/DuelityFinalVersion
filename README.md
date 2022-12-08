# Duelity Final Project

Game Objective

Duelity is a simple arcade like game where two opponents blend in with their environment and try to shoot the other player. Once one person is tagged with a bullet, the other player wins. There will be fun environments for players to use their hiding skills. 

Find your opponent and tag them with your bullet spawner before they can do the same for you. Blend into your environment as your movement will be very slow. After every point is scored, your positions will reset into new spawn points. The first one to ten points is the winner.

To help find your opponent, look for a COMSTAT item to provide updates on your enemy’s location.

New Features

Feature #1: Updated POV and Gun Assets

What worked: Implementing a better movement system with a look around system works well in a single player environment. I was also able to import a free sniper asset into Unity and attach it to the prefab with no problem. I was then able to add a more updated bullet spawner and camera to make it look like it was an FPS. Lastly, I was able to make a zoom script to allow the player to zoom in with their gun if they would like to.

What didn’t work: I was not able to convert my updated movement and look scripts to a network system. I also was not able to add a sniper delay to the bullet spawner. I attempted to use a Start Coroutine function to attempt to have the shoot function to wait but it was not read correctly.

I had to solve how to incorporate my movement scripts into a host/client system which did not unfortunately turn out well. I unexpectedly ran into a lot of problems with the prefab not registering the transformations in the same manner which became too much of a headache to convert. 

Feature #2: HUD of COMSTAT Pickup

What worked: I was able to attach to the prefab a HUD that would activate only when an item was picked up. This allowed for the player to see an update on their opponent on their screen.

What didn’t work: I was not able to track any player shooting or hiding in bushes. The hiding in bushes missing update came from the lack of finding a good bush asset to use for my project. I was solely focused on having this item act as a hint rather than a cheat code. 

Something interesting that I had to solve was how to calculate the distance between two players. At first, I thought I was clever by implementing Pythagorean theorem using the points of both players but that doesn’t factor in the height differential. Sadly, I had to use google and the distance formula worked as the correct math equation. The distance formula is the square root of all three points minus the second points position squared. Just google distance formula if you want to see the actual equation. The last challenge was converting the equation into a code friendly manner which turned into an absolute converting mess. Regardless, the implementation worked by using Math.f(Sq.rt()) and Math.f(Sq.Pow()). 

Features that I want to implement later

-Random spawn points: I really wanted to make it so that the spawn points were random for the players to spawn in so that there wouldn’t be any idea of what part of the map they are on. 

-Respawning when shot: This feature goes together with the previous one, if a player got shot, I wanted them to spawn somewhere else so that they would have to be found again.

-Implementing different maps: This feature would simply add variety to the game since playing on the same map can get boring.

-Implementing a better POV: As previously mentioned in Feature #1, I attempted to improve the POV of the player, but it was too much of a challenge to implement from a networking standpoint

-Adding a delay to shooting the sniper: This feature would make it so that there would be some level of punishment for spamming the bullets. You could also implement ammunition as well though.

-Choosing the number of opponents: I think it would be cool to have 1v1v1s or 2v2s to have much more depth in variety as well.
