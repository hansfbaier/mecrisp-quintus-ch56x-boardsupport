#require flash.fs
#require dump.fs
#require global.fs

: fstore ( data addr -- )
  !
;

: hfstore ( data addr -- )
  \ is the address in the upper halfword?
  dup %10 and if
    2dup 2- h@      \ get lower halfword data ( data addr data olddata )
    swap            ( data addr olddata data )
    $10 lshift      \ move data into the upper halfword
    or              \ and combine ( data addr newdata )
    swap            ( data newdata addr )
    %11 not and     \ word align write address
  else
    2dup @          \ get old data   ( data addr data olddata )
    $ffff0000 and   \ clear lower halfword
    swap            ( data addr olddata data )
    $ffff and       \ mask out lower halfword
    or              \ and combine ( data addr newdata )
    swap            ( data newdata addr )
  then
  !
  drop
;

: savetoflash ( start-addr )
  dup $5000 < if ." start address must at least $5000 " exit then
  ." storing flash dictionary in rom" cr
  begin
    dup dup @ swap    ( addr data addr )
    rom-write-word    ( addr )
    2 delay
    [char] . emit
    dup $ff and 0= if
      base @ 16 base ! \ store base
      over             \ get address
      cr
      u.               \ print it
      base !           \ restore base
    then
    4 +
  dup here >= until
  ." writing rom done. reset." cr cr
  reset
;

: eraseflash
  $5000 rom-erase-4k
  20 delay
  $6000 rom-erase-4k
  20 delay
  $7000 rom-erase-4k
  20 delay
  ." erased flash memory. Reset." cr cr
  10 delay
  reset
;

: flashhooks
['] fstore hook-flash! !
['] hfstore hook-hflash! !
;

flashhooks

: init
  flashhooks
  ." flash hooks initialized. " cr
;