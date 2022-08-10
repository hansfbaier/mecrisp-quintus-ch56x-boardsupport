#require global.fs
#require gpio.fs
#require tmr.fs

init-leds
leds toggle
led2 toggle

: blink-handler
  TMR1 RB_TMR_IE_CYC_END tmr-get-intflag
  if
    TMR1 RB_TMR_IE_CYC_END tmr-clear-intflag
    leds toggle
    ." blink "
  else \ ." ."
  then
;

' blink-handler irq-timer1 !

decimal 12000000 TMR1 timer-init

: pollblink begin blink-handler key? until ;

TMR1 timer-enable-interrupt
TMR1_IRQn pfic-enable-irq
