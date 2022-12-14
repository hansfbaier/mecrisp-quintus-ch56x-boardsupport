\ tft-scroll.txt
\ Part of pixelmaler-v0.1 for Mecrisp-Quintus - A native code Forth implementation for RISC-V
\ Copyright (C) 2018  Matthias Koch
\ display longan nano
\ MB March 2020

\ -----------------------------------------------------------
\ ---- To use the scroll capabilities three steps are -------
\ ---- needed: 1. Set the scroll areas 2 . Fill the   -------
\ ---- scroll area(s) 3. Move the scroll area. Each   -------
\ ---- move has to be started by programm.            -------
\ -----------------------------------------------------------

#require graphics/tft-fonts.fs

\ -----------------------------------------------------------
\ Something to display in scroll demo
\ In German it is a littel pun. "Guten Tag!" scrolls to
\ "Taten!" "Guten Tag!" = Hello! Good day!
\ Taten = deeds, like in "Doing a good deed every day."
\ -----------------------------------------------------------
  : test-gruss ( -- )
      big-font
      char-bg-col
      155 0 5 font-height @ filled-rectangle
      0 0 tft-at
      s" Guten Tag!" tft-type
  ;

\ Defaults for scroll variables
98 Variable scrollheight
33 Variable scrollstart
33 Variable bottomarea

\ set-scroll tell the scroll module where scroll area starts
\ and how high it is.
: set-scroll ( start height -- ) \
    2dup
    scrollheight !
    scrollstart !
    + tft-width @ 2 + swap - \ top
    >r
    scrollstart  @ $100 /mod
    scrollheight @ $100 /mod
    r> $100 /mod
    scrlar ( bottom scroll top -- ) \ set scroll area
;

\ One can fill borders if it is wanted.

true Variable borders

: fill-bottom-border ( -- )
    0 0 bottomarea @ 1- tft-width @
    filled-rectangle
;

: fill-top-border ( -- )
    tft-width @ bottomarea @ -
    0
    bottomarea @
    tft-width @
    filled-rectangle
;

: fill-borders? ( -- )
    big-font
    borders @
    IF
        yellow fill-top-border
        green fill-bottom-border
    THEN
;

\ set the scanline from that the area will be displayed
0 Variable scrollposition

: scroll+ ( n -- )
    scrollposition @
    + scrollheight @ +
    scrollheight @ mod
    dup scrollposition !
    scrollstart @ +
    0 vscsad            \ scroll start address of RAM
;

\ set the speed of scrolling
100 Variable scrollspeed
10 Variable +-scrollspeed

: scrollspeed- ( -- )
    scrollspeed @ +-scrollspeed @ +
    1000 min
    dup .
    scrollspeed !
;

: scrollspeed+ ( -- )
    scrollspeed @ +-scrollspeed @ -
    10 max
    dup .
    scrollspeed !
;

\ some key actions
: key-action? ( -- flag )
    key?
    IF key
        CASE
            [char] +  OF scrollspeed+ true ENDOF
            [char] -  OF scrollspeed- true ENDOF
            [char] t  OF -1 *  true ENDOF
            [char] q  OF false ENDOF
            [char] p  OF key drop true ENDOF
        ENDCASE
    ELSE true
    THEN
;

: display-key-actions ( -- )
    white char-bg-col!
    black char-fg-col!
    big-font
    2 0 tft-at
    s" +increase -decrease"
    tft-type tft-cr
    s" t toggle direction"
    tft-type tft-cr
    s" p pause    q quit "
    tft-type
    tft-cr
;

\ scroll the area given by start and heigth
: scroll ( start height n -- ) \ scroll
    -rot
    set-scroll
    fill-borders?
    BEGIN
    key-action?
    WHILE
      scrollspeed @ 500 * us
            pause
            dup
            scroll+
    REPEAT
    noron \ display command: normal mode
;

\ something more to display
: scroll-text ( -- )
    red char-fg-col!
    black char-bg-col!
    test-gruss
    display-key-actions
;

\ Three kinds of scrolling
: scroll-demo ( -- )
    white tft-clear
    scroll-text
    3000 ms
    true borders !
    33 160 66 - 1 scroll
    white tft-clear
    scroll-text
    false borders !
    33 160 66 - 1 scroll
    false borders !
    1 160 -1 scroll
;
