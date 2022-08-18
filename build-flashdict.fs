#include flashhooks.fs
compiletoflash

\ uncomment this if you want to save memory and
\ skip some convenience words
\ 1 constant lite

#include flashhooks.fs

\ basics
#require gpio.fs
#require tmr.fs
#require spi.fs
#require multitask.fs

\ debugging tools
#require dump.fs
#require disass.fs

\ graphics
0 [if]
#require graphics/tft-basics.fs
#require graphics/tft-graphics.fs
#require graphics/turtle.fs

: init
  init
  tft-init    100 ms
  tft-clear   100 ms
  color-demo  100 ms
  turtle-demo 100 ms
  ." press 'q' to quit graphics demo " cr
  graphics-demo
;
[then]

char m emit char e emit char m emit char : emit space
unused . cr

savetoflash