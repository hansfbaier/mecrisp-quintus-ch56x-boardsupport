#require flash.fs
#require dump.fs

: delay ( ms -- )
  $2ee0 * \ 12000
  begin 1- dup 0= until
  drop
;

: fstore ( data addr -- )
  2dup !
  rom-write-word
  1 delay
;

' fstore hook-flash! !

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
  2dup !
  rom-write-word
  drop
  1 delay
;

' hfstore hook-hflash! !

0 variable cnt
: eraseflash
  0 cnt !
  begin
    cnt @ $f and 0= if hex cnt @ u. then
    $ffffffff
    $4c00 cnt @ 2 lshift +
    flash!
    1 delay
    cnt @ 1+ dup cnt !
  $2d00 = until
;

