\ tft-basics.txt
\ Part of pixelmaler-v0.1 for Mecrisp-Quintus - A native code Forth implementation for RISC-V
\ Copyright (C) 2018  Matthias Koch
\ Copyright (C) 2022  Hans Baier (adaptation to CH56x)
\ ST7735 128x160 display
\ MB March 2020
\ HB August 2022

\ This is work in progress!
\ It needs 'ms', one of the tree files xy.txt, xy-hard.txt
\ or xy-high-level.txt. I recommend xy-hard.txt.
\ tft-basics.txt includes the words to comminicate with the
\ driver chip ST7735S.
\ txt-basics.txt allowes you to use the the examples graphics-unicode-8x16.txt
\ and graphics-unicode-8x8.txt. You find them in
\ mecrisp-quintus-experimental-xxxx-/gd32vf103cb/common
\ How to implement see end of file!
\
\ There are some useful enhancments in different files:
\ txt-graphs.txt -- lines, circles, rectangles
\ txt-fonts.txt  -- chars, strings, and different UTF8-fonts
\ txt-scroll.txt -- An example how to use the built in scroll function
\ txt-bmp.txt    -- An example how to show BMP-files
\ taxi.fs        -- like turtle graphics but using taxi geometry also
\                   called manhattan geometry
\ turtle.fs      -- a real turtle graphic but without 'hide turtle' and
\                   'show turtle' . To include it you must have the
\                    fixed point math installed. (common/fixpt-math-lib.fs)

\ If you include spi0-hard.txt you can only use the functions
\ that are accessible by writing to the driver chip. This is
\ due to the special 3/4-wired interface and the fact that reading
\ from the driver device needs a extra 'dummy clock circle'. (See spi0-soft.txt)

\ If you include spi0-soft.txt you get some extra functionality. Especially
\ query chip-ID, configuration and so on. The price for it:
\ It will be way slower!

\ see: driver chip ST7735S_V1.5_20150303.pdf p44
\ the tft-chip is wired as a 4-line connection
\ with "... CSX (chip enable), D/CX (data/command flag), SCL
\ (serial clock) SDA (serial data input/output). Serial clock
\ (SCL) is used for
\ interface with MCU only ..."

\ The different names of standard SPI (MISO MOSI CS CKL) the
\ schematic (longan nano 2663(schematic).pdf) and the data sheet
\ of the driver chip (driver chip st7735s_v1.5_20150303.pdf) are
\ somewhat confusing. So i've decided to use the names of the
\ datasheet See: spi0-soft.txt and/or spi0-hard.txt

#require spi.fs
#require graphics/xy-hard.fs

[ifndef] TFT_SPI
  SPI0 constant TFT_SPI
[then]

: -tft-csx spi0-cs-low ;
: +tft-csx spi0-cs-high ;

: -tft-d/cx ( -- )
  [ pin #13 literal, ] PortB CLEAR gpio-set
;

: +tft-d/cx ( -- )
  [ pin #13 literal, ] PortB OUT gpio-set
;

: -tft-rst ( -- )
  [ pin #12 literal, ] PortB CLEAR gpio-set
;

: +tft-rst ( -- )
  [ pin #12 literal, ] PortB OUT gpio-set
;

: tft-gpio-init
  \ RST
  [ pin #12 literal, ]
  PortB DIRECTION +
  bis!
  \ DC
  [ pin #13 literal, ]
  PortB DIRECTION +
  bis!
;

: raw-byte>tft ( n -- )
    -tft-csx
    TFT_SPI spi-tx
    +tft-csx
;

: byte>tft ( n -- )
    +tft-d/cx
    raw-byte>tft
;

: 2bytes>tft ( n -- )
    +tft-d/cx
    $100 /mod
    raw-byte>tft
    raw-byte>tft
;

: tft-reg ( n -- )
    -tft-d/cx
    raw-byte>tft
;

\ the tft basics

decimal

\ command: is a defining word. It generates commands send to the tft-driver
\ chips. Use as follow: Put the number of parameters to be send after the
\ command on stack, put the command (register) on stack. Then type
\ command: NAME to create the new word NAME i.e. command  send to the tft.
\ The naming is strictly orientated at the datasheet. I think of them as
\ semi-primitives. Cave at! Parameter passing in reverse order.
\ see: driver chip ST7735S_V1.5_20150303.pdf: pp 104


: command: ( name ) ( parfn ... parf1 nb nf tft-reg -- )
    <builds , \ register
    h,       \ nb
   does>
    dup @ tft-reg 4 +
     h@ 0 ?DO byte>tft LOOP
   ;

\ define write commands only. for reading see spi0-soft.txt

   \ System function Commands
   0 $01 command: swreset ( -- )
   0 $10 command: slpin   ( -- )                                       \ sleep in @ booster off
   0 $11 command: spout   ( -- )                                       \ sleep-out @ booster on
   0 $12 command: ptlon   ( -- )                                       \ partial mode on
   0 $13 command: noron   ( -- )                                       \ partial mode off (normal)
   0 $20 command: invoff  ( -- )                                       \ display inversion off (normal)
   0 $21 command: invon   ( -- )                                       \ display inversion on
   1 $26 command: gamset  ( b -- )                                     \ gamma curve select
   0 $28 command: dispoff ( -- )                                       \ display off
   0 $29 command: dispon  ( -- )                                       \ display on
   4 $2A command: caset   ( h-byte1 low-byte1 h-byte2 low-byte2 -- )   \ column address set
   4 $2B command: raset   ( h-byte1 low-byte1 h-byte2 low-byte2 -- )   \ row address set

   0 $2c command: ramwr \ write meory i.e. start writing,
                        \ all consecutive bytes must be send via byte>tft
                        \ or 2bytes>tft (in my case this latter is used (16-bit pixel)

 128 $2D command: rgbset  ( red_tone*$a0 green_tone*$b0 blue_tone*$a0 -- )           \ LUT for color display \ can't understand 128 ???
   4 $30 command: ptlar   ( h-byte1 low-byte1 h-byte2 low-byte2 -- )                 \ partial start/end address set
   6 $33 command: scrlar  ( top-fixed-area height-scroll-area bottom-fixed-area -- ) \ scroll area set
   0 $34 command: teoff   ( -- )                                                     \ tearing effect line off; not connectet with nano
   1 $35 command: teon    ( b -- )                                                   \ tearing effect mode set & on; not connected with nano
   1 $36 command: madctl  ( b -- )                                                   \ memory data access control
   2 $37 command: vscsad  ( b b -- )                                                 \ scroll start address of RAM
   0 $38 command: idmoff  ( -- )                                                     \ idle mode off
   0 $39 command: idmon   ( -- )                                                     \ idle mode on
   1 $3a command: colmode ( b -- )                                                   \ color mode

   \ Panel Function Commands
   3 $b1 command: frmctr1 ( b b b  -- )      \ in normal mode
   3 $b2 command: frmctr2 ( b b b -- )       \ in idle mode
   6 $b3 command: frmctr3 ( b b b b b b -- ) \ in partial mode & full colors
   1 $b4 command: invctr  ( b -- )           \ display inversion control
   3 $c0 command: pwctr1  ( b b b -- )       \ power contrl setting; GVDD voltage
   1 $c1 command: pwctr2  ( b -- )           \ power control setting; VGH/VGL voltage
   2 $c2 command: pwctr3  ( b b -- )         \ power control setting; in normal mode full colors; adjust amplifier & booster voltage
   2 $c3 command: pwctr4  ( b b -- )         \ power control setting; in idle mode 8-colors; adjust amplifier & booster voltage
   2 $c4 command: pwctr5  ( b b -- )         \ power control setting; in idle partial mode full colors; adjust amplifier & booster voltage
   1 $c5 command: vmctr1  ( b -- )           \ VCOM control 1, voltage
   1 $c7 command: vmofctr ( b -- )           \ set VCOM offset control ;  see: nvctr1-3
   1 $d1 command: wrid2   ( b -- )           \ set LMC version code at ID2 (fixed?) see: nvctr1-3
   1 $d2 command: wrid3   ( b -- )           \ set customer project code at ID3 (fixed?)  see: nvctr1-3

   \ Note 2: The D9h, Deh and DFh registers are used for NV Memory function controller. (Ex: write, clear, etc.)
    1 $d9 command: nvctr1    ( b -- )         \ NVM control status; must be supplied with external 7.5V
    2 $de command: nvctr2    ( b b -- )       \ NVM read command; b1=F5 b2=A5 action code .i.e don't read something via SPI?
    2 $df command: nvctr3    ( b b -- )       \ NVM write command; b=A5 action code
   16 $e0 command: gamctrp1  ( b1 .. b16 )    \ set gamma adjustment +polarity
   16 $e1 command: gamctrn1  ( b1 .. b16 )    \ set gamma adjustment -polarity
    1 $fc command: gcv       ( b -- )         \ gate clock varaible

   \ end of semi-primitives now starting high-level

\ overwrite madctl    \ because we couldn't read the real register with hardware spi module
$78 Variable (madctl) \ we need a shadow register. spi0-soft is able to read from the ST7735S
                      \ So if you want to have read acces to the display use spi0-soft.txt
   : madctl ( n -- )  \ and comment out this lines
       dup (madctl) ! \   "
       madctl         \   "
   ;

   : f-rate-normal ( -- )   \ Frame rate 1 normal mode: line period; front porch, back porch
        $3A $3A $05 frmctr1 ;

    : f-rate-idle ( -- )    \ Frame rate 2 idle mode: line period; front porch, back porch
        $3A $3A $05 frmctr2 ;

    : f-rate-partial ( -- )   \ Frame rate 3 dot inversion mode and column inverted mode
        $3a $3a $05 $3a $3a $05 frmctr3 ;

    : inversion-control ( -- )  \ Display inversion control: %111= inversion in full color, idle and partial
        $03 invctr ;

    : power1 ( -- )  \ Powerctrl 1; AVDD=4.8V; GVDD=4.6V; ???; Mode=2X
        $04 $02 $62 pwctr1 ;

    : power2 ( -- )   \ Power Control 2 = ?????
        $c0 pwctr2 ;

    : power3 ( -- )   \ Power control 3
        $00 $0d pwctr3 ;

    : power4 ( -- )   \ Power control 4
        $6a $8d pwctr4 ;

    : power5 ( -- )   \ Power control 5
        $ee $8d pwctr5 ;

    : vcom ( -- )       \ VCOM Control 1: 001110=-0.775 V
        $0e vmctr1 ;

    \   $10 $0e $02 $03 $0e $07 $02 $07 $0a $12 $27 $37 $00 $0d $0e $10       16 0 $e0
    : +Gamma ( -- )
        $10 $0e $0d $00 $37 $27 $12 $0a $07 $02 $07 $0e $03 $02 $0e $10 gamctrp1 ;

\       $10 $0e $03 $03 $0f $06 $02 $08 $0a $13 $26 $36 $00 $0d $0e $10         16 0 $e1 ;
    : -Gamma  ( -- )
        $10 $0e $0d $00 $36 $26 $13 $0a $08 $02 $06 $0f $03 $03 $0e $10 gamctrn1 ;

    : 16bit/pixel ( -- ) \ full color interface pixel format: 101=16bit/pixel
        $05 colmode ;

    : memory-access   ( -- ) \ memory data access: 1111000 = MX:colum adress; MV:Row colum Exchange
        $78  madctl ; ( -- ) \ ML:vertical Refresh bottom to Top  RGB:GBR

    : write-memory ( -- ) \
        ramwr ;

    : column-address! ( n n -- )
        swap $100 /mod rot $100 /mod caset ;

    : row-address! ( n n -- )
        swap $100 /mod rot $100 /mod raset ;


\ In tft-init there is a use of 'ms'. If you don't have a 'ms' in your system
\ replaces it by DO LOOP. Maybe you have to fiddle a litle bit to find
\ values that fit.

: tft-init ( -- )
  tft-gpio-init
  TFT_SPI spi-master-init

  -tft-rst 200 ms
  +tft-rst 20 ms
  \ ------------------------------------
  spout 100 ms
  f-rate-normal f-rate-idle f-rate-partial
  inversion-control
  power1 power2 power3 power4 power5 Vcom
  +Gamma -Gamma
  16bit/pixel
  memory-access
  dispon
;

\ Orientations: Thre are at leas 16 combinations. One can choose
\ between landscape and portrait. The both came with 'normal' or
\ bottom-up orientation. Also it is possible to mirror horizontally
\ and vertically.



\ MADCTL:
\  MY  MX  MV  ML  RGB  MH  -- --
\  |   |   |   |    |   |   |  +-- don't care
\  |   |   |   |    |   |   +----- don't care
\  |   |   |   |    |   +--------- Horizontal Refresh Order
\  |   |   |   |    +------------- RGB-BGR ORDER
\  |   |   |   |
\  |   |   |   +------------------ Vertical Refresh Order
\  |   |   +---------------------- Row/Column Exchange
\  |   +-------------------------- Column Address Order
\  +------------------------------ Row Address Order

\ After introducing the fonts there will be a word
\ 'show-orientations' that helped me to figure out
\ which orientation I want to use.

[ifndef] TFT_LONG_SIDE
160 constant TFT_LONG_SIDE
[then]

[ifndef] TFT_SHORT_SIDE
128 constant TFT_SHORT_SIDE
[then]

[ifndef] tft-width
TFT_LONG_SIDE Variable tft-width
[then]

[ifndef] tft-height
TFT_SHORT_SIDE Variable tft-height
[then]

[ifndef] orientation
-1 Variable orientation
[then]

: orientation-landscape ( -- )
    TFT_LONG_SIDE  tft-width  !
    TFT_SHORT_SIDE tft-height !
    true orientation !
;

: orientation-portrait ( -- )
    TFT_SHORT_SIDE tft-width  !
    TFT_LONG_SIDE  tft-height !
    false orientation !
;

: landscape ( -- )
    $78 madctl  \ %01111000
    orientation-landscape
;

orientation-landscape
\ landscape

: landscape-bottom-up
    $a8 madctl
    orientation-landscape
;

: portrait
    $C8 madctl
    orientation-portrait
;

: portrait-bottom-up
    $18 madctl
    orientation-portrait
;

: mirror-v ( -- )
    orientation @
    IF 64 (madctl) cxor!
    ELSE 128 (madctl) cxor!
    THEN
    (madctl) @ madctl
;

: mirror-h ( -- )
    orientation @
    IF 128 (madctl) cxor!
    ELSE 64 (madctl) cxor!
    THEN
    (madctl) @ madctl
;

\ ----------------------------------------------------------------
\ ---- color management ------------------------------------------
\ ----------------------------------------------------------------

\ The format is: RGB 5-6-5 i.e. 5 bit red, 6 bit green, 5 bit blue
\ The word rgb: takes a color in HTML format, calculates the
\ 16-bit value for the TTF-Display and creates a word for this
\ color.
\ This value is also indexed in a buffer. This makes it
\ easy to reference the colors by the index number.
\ One can use either 'blue' or '14 color!' to set the current
\ color to blue.
\ If you switch to compiletoflash the buffer will be in flash
\ and the colors will be 'built in'. I.e. you can't change the
\ color-buf later. Of corse you can create new colors further
\ on and store them in a separate ram buffer (word buffer: )

%0000011111100000 Variable color

: rgb ( rgb -- n )
    $100 /mod $100 /mod
    %11111  $FF */ -rot
    %111111 $FF */ 5 lshift -rot
    %11111  $FF */ 11 lshift
    + +
;

: rgb: ( name ) ( r g b/- -- )
    rgb \ dup >color-tab
    <builds ,
  does> @ color !
;

$000000 rgb: black      \ 0
$800000 rgb: maroon     \ 1
$00FF00 rgb: green      \ 2
$808000 rgb: olive      \ 3
$000080 rgb: navy       \ 4
$800080 rgb: purple     \ 5
$008080 rgb: teal       \ 6
$a9a9a9 rgb: darkgray   \ 7
$C0C0C0 rgb: silver     \ 8
$ff0000 rgb: red        \ 9
$00FF00 rgb: lime       \ 10
$228B22 rgb: forestgreen \ 11
$FFFF00 rgb: yellow      \ 12
$0000FF rgb: blue        \ 13
$FF00FF rgb: fuchsia     \ 14
$00FFFF rgb: aqua        \ 15
$FFFFFF rgb: white       \ 16

: >color-tab ( -- )
    color @ h,
;

Create color-tab 17 h,
  black       >color-tab \ 0
  maroon      >color-tab \ 1
  green       >color-tab \ 2
  olive       >color-tab \ 3
  navy        >color-tab \ 4
  purple      >color-tab \ 5
  teal        >color-tab \ 6
  darkgray    >color-tab \ 7
  silver      >color-tab \ 8
  red         >color-tab \ 9
  lime        >color-tab \ 10
  forestgreen >color-tab \ 11
  yellow      >color-tab \ 12
  blue        >color-tab \ 13
  fuchsia     >color-tab \ 14
  aqua        >color-tab \ 15
  white       >color-tab \ 16

: #colors ( -- n )
    color-tab h@
;

: color! ( n -- )
    color-tab h@
    min 0 max
    1+ 2* color-tab + h@
    color !
;

\ basic graphics

\ set-shape defines the area to be filled
\ Due to capabilities of the ST7735S a point is a region with
\ all sides set to one.

: set-shape ( x y x' y' -- )
    orientation @
    IF
      rot row-address!       \ x landscape
      swap column-address!    \ y
    ELSE
      rot row-address!       \ x portrait
      swap column-address!  \ y
    THEN
;

\ tft-fill filles the current defined area with color

: tft-fill ( width height -- )
    write-memory
    1 + swap 1 + * 0 ?DO color @ 2bytes>tft LOOP
;

\ -------------------------------------------------------
\ -------- rectangles are somewhat complicated ----------
\ -------- because the physical display is 162x80 -------
\ -------- pixels. Beside this you have to sub- ---------
\ -------- stract 1 pixel in width and heigth. ----------
\ -------- This is due to the IT counting starting ------
\ ---------with zero. -----------------------------------
\ -------------------------------------------------------

\ a filled rectangle

: filled-rectangle ( x y width height -- )
    2tuck          \ copy width height for later fill
    -1 -1 xy+      \ correct width heigt (IT counting)
    xywh+ set-shape
    tft-fill
;

\ 'clear' the tft display. I.e. fill it with color

: tft-clear ( -- )
    0 0 tft-width @ tft-height @
    filled-rectangle
;

: color-demo ( -- )
    black TFT_LONG_SIDE 0 10 TFT_SHORT_SIDE filled-rectangle
    color-tab h@
    0 ?DO
        I color!
        tft-width @ color-tab h@ /   \ w
        dup I * swap               \ x w
        0 swap                     \ x y w
        tft-height @                  \ x y w h
        filled-rectangle
    LOOP
;
