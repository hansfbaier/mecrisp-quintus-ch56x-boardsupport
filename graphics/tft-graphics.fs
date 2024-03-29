\ tft-graphics.txt
\ Part of pixelmaler-v0.1 for Mecrisp-Quintus - A native code Forth implementation for RISC-V
\ Copyright (C) 2018  Matthias Koch
\ display longan nano
\ MB March 2020

#require graphics/tft-basics.fs

\ ----------------------------------------------------------------
\ -------------- basic graphic routines --------------------------
\ ----------------------------------------------------------------

\ points and lines

1 Variable thickness

: draw-point ( x y -- )
    thickness @ dup filled-rectangle
;

: h-line ( x y width -- )              \ draw horizontal line
    thickness @ filled-rectangle
;

: p-line ( x y height )                \ draw perpendicular line
    thickness @ swap filled-rectangle
;

\ This is the classic bresenham line algorithm as it can be
\ found in WIKIPEDIA. I took it from Matthias's
\ common/graphics-unicode-8x8.txt. Some changes were made due to
\ the fact that on tft vertical and horizontal lines are handled
\ as rectangels and in this way are drawn faster than consequent
\ singel pixels. Look at the difference between put-pixel and
\ put-pixels!
\ If you want to use the pure bresenham algorithm comment out
\ all lines marked with \ !!! at the end.

\ -------------------------------------------------------------
\ ---------------- Bresenham line(s) --------------------------
\ -------------------------------------------------------------

0 variable line-x1   0 variable line-y1
0 variable line-sx   0 variable line-sy
0 variable line-dx   0 variable line-dy
0 variable line-err
0.0 2variable last-xy

\ draw-piece-of-line draws a (short?) perpendicular or
\ horizontal line
: draw-piece-of-line ( x0 y0 x1 y1 -- )
     xy-minmax
     2over 2over
     set-shape
     2swap xy-
    tft-fill
;

\ put-pixels uses three pairs of x-y coordinates.
\ It compares these coordinates. If they are all
\ collinear the oldest and the newest ones will
\ remain on the stack.
\ If one deviates (the newest one) the line between
\ the oldest ones will be drawn and the doubled
\ new point laid on stack.
: put-pixels  ( x'' y'' x' y' x y -- x'' y'' x' y' )
    4 pick <>              \ compare y's
    swap 5 pick <>         \ compare x'ses
    and                    \ changed?
    IF  2swap              \ get start point
        last-xy 2@         \ @ last point
        draw-piece-of-line \ draw-line
        2dup               \ douple new point (start end)
    THEN                   \
    2dup last-xy 2!        \ new point becomes last point
;

: put-pixel ( x y -- )
    2dup
    1 1 xy+
    set-shape
    write-memory
    color @ 2bytes>tft
;

\ p-line? draws a perpendicular line
\ if possible consume the parameters and give back TRUE
\ if not possible conserve the parameters and give back FALSE

: p-line? ( x0 y0 x1 y1 -- {x0 y0 x1 y1} f )
    over 4 pick =
    IF xy-minmax
        2over 2over set-shape
        2swap xy- nip 1 tft-fill
        TRUE
    ELSE FALSE
    THEN
;

\ h-line? draws a horizontal line
\ if possible consume the parameters and give back TRUE
\ if not possible conserve the parameters and give back FALSE
: h-line? ( x0 y0 x1 y1 -- {x0 y0 x1 y1} f )
    dup 3 pick =
    IF  xy-minmax
        2over 2over set-shape
        2swap xy- drop 1 tft-fill
        TRUE
    ELSE FALSE
    THEN
;

\ This bresenham line
\ - looks if the line is orthogonal
\ - draws pieces of orthogonal lines
\ The worst case are lines near an angel of 45 degrees.
\ That raises a lot of overhead.
: line ( x0 y0 x1 y1 -- )
    p-line? IF exit THEN \ !!!
    h-line? IF exit THEN \ !!!

    2over last-xy 2! \ !!!
    line-y1 ! line-x1 !

    over line-x1 @ -   dup 0< if 1 else -1 then line-sx !   abs        line-dx !
    dup  line-y1 @ -   dup 0< if 1 else -1 then line-sy !   abs negate line-dy !
    line-dx @ line-dy @ + line-err !
    2dup \ !!!

    begin
       2dup put-pixels  \ !!! replace put-pixels with put-pixel
       2dup line-x1 @ line-y1 @ d<>

    while
       line-err @ 2* >r
       r@ line-dy @ > if line-dy @ line-err +! swap line-sx @ + swap then
       r> line-dx @ < if line-dx @ line-err +!      line-sy @ +      then
    repeat
    2drop
    last-xy 2@  draw-piece-of-line \ !!!
;

\ -------------------------------------------------------------
\ --- testing the line algorithm with moiree ------------------
\ -------------------------------------------------------------

tft-width @ 2/ tft-height @ 2/ 2Variable midpoint
0 variable moiree-col
: switch-color ( -- )
    moiree-col @ color-tab h@ 3 - mod
    1 + color!
    1 moiree-col +!
;

: moiree ( -- )
   tft-width @ 0 DO switch-color midpoint 2@ I 0 line LOOP
   tft-height @  0 DO switch-color midpoint 2@ 160 I  line LOOP
   tft-width @ 0 DO switch-color midpoint 2@ tft-width @ I -  80 line LOOP
   tft-height @  0 DO switch-color midpoint 2@ 0 tft-height @  I -  line LOOP
;

\ ----------------------------------------------------------------
\ -------------- bresenham ellipse(s) and circle(s) --------------
\ ----------------------------------------------------------------

\ This I've also taken from Matthias's
\ common/graphics-unicode-8x8.txt.
\ Destroyed some formattings - to fit into 64 columns.
\ I expanded it with a filled mode. see mirror

\ -------------------------------------------------------------
\  Bresenahm ellipse
\ -------------------------------------------------------------

0 variable ellipse-xm   0 variable ellipse-ym
0 variable ellipse-dx   0 variable ellipse-dy
0 variable ellipse-a    0 variable ellipse-b
0 variable ellipse-a^2  0 variable ellipse-b^2
0 variable ellipse-err

\ instead of writing two versions of ellipse I decided to use
\ a kind of deferred word. The main word ellipse-mirror executes
\ the CFA of three words stored in the variable (ellipse-mirror)
\ This variable is intialised with a error message.

: .mirror-empty ( -- )
  ." First choose the drawing mode (outlined; filled)!"
  quit
;

' .mirror-empty  Variable (ellipse-mirror)

: ellipse-mirror ( -- )
    (ellipse-mirror) @ execute ;

\ ellipse-put-pixel transforms the coordinates of a point
\ than it draws the point
: ellipse-putpixel ( y x -- )
    ellipse-xm @ + swap ellipse-ym @ + put-pixel
;

\ ellipse-line transforms the coordinates of two points
\ than draws a line between this points
: ellipse-line ( y x y' x' -- )
    ellipse-xm @ ellipse-ym @ xy+ swap 2swap
    ellipse-xm @ ellipse-ym @ xy+ swap 2swap
    xy-minmax 2over 2over set-shape
    2swap xy- tft-fill
;

\ ellipse-mirror-points calculates (transfoms) the point stored
\ in ellipse-dx and ellipse-dy, mirrors it into al four quadrants
\ and draws them
: ellipse-mirror-points ( -- )
    ellipse-dy @        ellipse-dx @        ellipse-putpixel
    ellipse-dy @ negate ellipse-dx @        ellipse-putpixel
    ellipse-dy @ negate ellipse-dx @ negate ellipse-putpixel
    ellipse-dy @        ellipse-dx @ negate ellipse-putpixel
;

\ ellipse-mirror-lines calculates (transfoms) the point stored
\ in ellipse-dx and ellipse-dy, mirrors it into all four quadrants
\ and connect teh pairs of points with a line. (filled mode)
: ellipse-mirror-lines ( -- )
    ellipse-dy @        ellipse-dx @
    ellipse-dy @ negate ellipse-dx @        ellipse-line
    ellipse-dy @ negate ellipse-dx @ negate
    ellipse-dy @        ellipse-dx @ negate ellipse-line
;

\ outlined and filled may be better named outline and fill, but
\ fill is a forth standard word.
\ Or? outline and solid?


\ outlined switches the behaviour of ellipse-mirror so that
\ it draws lines
: outlined ( -- )
    ['] ellipse-mirror-points (ellipse-mirror) ! ;

\ filled switches the behaviour of ellipse-mirror so that
\ it draws single points
: filled ( -- )
    ['] ellipse-mirror-lines  (ellipse-mirror) ! ;

: ellipse-step ( -- )
    ellipse-mirror

    ellipse-err @ 2* >r
    r@  ellipse-dx @ 2* 1+ ellipse-b^2 @ *
    < if  1 ellipse-dx +! ellipse-dx @ 2* 1+ ellipse-b^2 @ *
        ellipse-err +!
    then
    r>  ellipse-dy @ 2* 1- ellipse-a^2 @ * negate
    > if -1 ellipse-dy +! ellipse-dy @ 2* 1- ellipse-a^2 @ *
        negate ellipse-err +!
    then
;


: ellipse ( xm ym a b -- )

  0 ellipse-dx ! dup ellipse-dy !

  dup ellipse-b ! dup * ellipse-b^2 !
  dup ellipse-a ! dup * ellipse-a^2 !
  ellipse-xm ! ellipse-ym !            \ exchange x y

  ellipse-b^2 @ ellipse-b @ 2* 1- ellipse-a^2 @ * - ellipse-err !

  begin
    ellipse-step
    ellipse-dy @ 0<
  until

  ellipse-dx @
  begin
    1+
    dup ellipse-a @ <
  while
    0 over        ellipse-putpixel
    0 over negate ellipse-putpixel
  repeat
  drop
;

: circle ( xm ym r -- ) dup ellipse ;

\ -------------------------------------------------------------
\ --- some demonstrations -------------------------------------
\ -------------------------------------------------------------

5 Variable b-diameter  \ diameter b of ellipse
20 Variable wait       \ time between drawings

: +wait ( -- )
   wait @ 1 + wait !
;

: -wait ( -- )
   wait @ 1 - 1 max wait !
;

: key-pressed? ( -- flag)  \ handles key strokes
    key?                   \ is a key pressed?
    IF                     \ if ...
    key                    \ which key
    case                   \ in case of ...
        [char] + of -wait FALSE endof \ accelerate
        [char] - of +wait FALSE endof  \ slow down
        [char] q of TRUE endof \ quit i.e. TRUE
        FALSE swap         \ ignore any other case
    endcase                \ endcase drops TOS
    ELSE FALSE             \ flag
    THEN
;

\ ellipse-b draws fixed ellipse with variable diameter b
: ellipse-b ( -- )
   40 40 30                  \ x y diameter-a
   30                        \ maximum of diameter-b
   b-diameter @ dup >r - abs \ fetch growing/shrinking diameter-b
   r> 60 mod b-diameter !    \ limit diameter-b
   ellipse
;

\ a animated ellipse, best results by speed 20 (in my case)
: ellipse-animation ( -- )
    BEGIN
        white ellipse-b
        1 b-diameter +!
        blue  ellipse-b
        wait @ ms
        key-pressed?
    UNTIL
;

: frame ( n -- )
    green tft-clear
    white dup dup 2* >r
    tft-width @ r@ -
    tft-height @ r> -
    filled-rectangle
;

: square ( x y a -- )
    dup filled-rectangle
;

: brick ( x y a b -- )
    filled-rectangle
;

: ball ( x y r -- )
    filled
    >r                \ x y x y
    color @ -rot
    2dup
    darkgray
    swap r@  3 4 */ + swap    \ x' y
    r@ 3 4 */ +             \ x' y'
    r@ 5 4 */         \ x' y' a
    r@ 2 4 */
    ellipse
    rot color !
    r> circle
;

\ haxgon; sin30°=0.5 i.e. 2/ cos30°=0.886025... i.e. 866 1000 */
: hexagon ( x y r -- )
    black
    >r
    ellipse-ym ! ellipse-xm ! \ reuse ellipses midpoint
    ellipse-xm @                  ellipse-ym @ r@ - \ P1
    2dup
    ellipse-xm @ r@ 886 1000 */ + ellipse-ym @ r@ 2/ - \ P2
    2tuck  line                      \ P1 P2 on stack
    over                          ellipse-ym @ r@ 2/ + \ P3
    2tuck line                     \ P1 P3 on stack
    ellipse-xm @                  ellipse-ym @ r@ +    \ P4
    2tuck line                     \ P1 P4 on stack
    ellipse-xm @ r@ 886 1000 */ - ellipse-ym @ r@ 2/ + \ P5
    2tuck line                     \ P1 P5 on stack
    ellipse-xm @ r@ 886 1000 */ - ellipse-ym @ r> 2/ - \ P6
    2tuck line                     \ P1 P6 on stack
    line
;

: graphics-demo ( -- )
    moiree
    3000 ms
    5 frame
    130 10 15 blue square
    75 10 10 60 maroon brick
    110 40 20 red ball
    outlined
    40 40 36 hexagon
    ellipse-animation
;

