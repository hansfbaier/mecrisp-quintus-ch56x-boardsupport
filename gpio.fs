compiletoram

$40001040 constant PortA
$40001060 constant PortB
$00 constant DIRECTION  \ RW, I/O direction: 0=in, 1=out
$04 constant IN         \ RO, input
$08 constant OUT        \ RW, output
$0C constant CLEAR      \ WZ, clear output: 0=keep, 1=clear
$10 constant PULLUP     \ RW, pullup resistance enable
$14 constant PULLDOWN   \ RW, output open-drain & input pulldown resistance enable
$18 constant DRIVE      \ RW, driving capability: 0=8mA, 1=16mA
$1C constant SCHMITT    \ RW, output slew rate & input schmitt trigger: 0=fast,normal, 1=slow,schmitt

\ convenience methods
\ this takes the next token as pin number
: pin ( -- bit_pattern )
  token number \ read next token and convert it to a number
  1 = if
    1 swap lshift
  else
    2 = if drop then
  then
;

: gpio-set    ( pins port_base port_offset -- ) + bis! ;
: gpio-clear  ( pins port_base port_offset -- ) + bic! ;
: gpio-toggle ( pins port_base port_offset -- ) + xor! ;

pin 22 constant led0
pin 23 constant led1
pin 24 constant led2

led0 led1 led2 or or constant leds

: led-init ( led -- )
  dup PortB DIRECTION gpio-set
  dup PortB PULLUP    gpio-clear
  dup PortB PULLDOWN  gpio-clear
  dup PortB SCHMITT   gpio-set
  dup PortB DRIVE     gpio-clear
      PortB OUT       gpio-set
;

: on  ( led -- ) PortB CLEAR gpio-set ;
: off ( led -- ) PortB OUT   gpio-set ;

: init-leds ( -- )
  led0 led-init
  led1 led-init
  led2 led-init
;

: toggle ( led -- )
  PortB OUT gpio-toggle
;

: delay ( ms -- )
  12000 *
  begin 1- dup 0= until
  drop
;

: blinky
  init-leds
  begin
    led0 toggle
    50 delay
    led1 toggle
    50 delay
    led2 toggle
    50 delay
  key? until
;