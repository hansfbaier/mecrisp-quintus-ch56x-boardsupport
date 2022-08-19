#require graphics/tft-basics.fs
#require graphics/font.fs

: hgray ( nibble -- rgb )
  dup #12 lshift swap ( r nibble )
  dup #7  lshift swap ( r g nibble )
  1 lshift
  or or
;

0 variable text-x
0 variable text-y

: tft-page
  0 text-x ! 0 text-y !
  tft-clear
;

: tft-cr
  text-y @ 6 + dup tft-height @ >
  if
    tft-page
  else
    text-y ! 0 text-x !
  then
;

: hexdigit ( d -- )
  text-x @ 5 + tft-width @ > if tft-cr then
  text-x @ text-y @  text-x @ 3 + text-y @ 4 +  set-shape
  text-x @ 5 + text-x !

  write-memory
  2 4 5 * * * micro +   ( 'firstpixel )
  [ 4 5 * literal, ] 0 do
    dup h@
    2bytes>tft
    2+
  loop
  drop
;

: mytext-demo
  tft-init
  black tft-clear
  0 text-x !
  0 text-y !
  7712 0 do
    i $f and hexdigit
  loop
;
