
\ Let it shine !

#require multitask.fs
#require gpio.fs

task: blink-led1

: blink-led1& ( -- )
  blink-led1 activate
    begin
      led1 toggle
      220 ms
    again
;

task: blink-led2

: blink-led2& ( -- )
  blink-led2 activate
    begin
      led2 toggle
      320 ms
    again
;

: blinky ( -- )
  init-leds
  multitask
  blink-led1&
  blink-led2&
;
