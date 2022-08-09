$40002000 constant TMR0  \ point TMR0 base address
$40002400 constant TMR1  \ point TMR1 base address
$40002800 constant TMR2  \ point TMR2 base address

\ register offsets relative to base address
$00 constant R32_TMR_CONTROL     \  RW, TMR control
$00 constant R8_TMR_CTRL_MOD     \  RW, TMR mode control
$02 constant R8_TMR_INTER_EN     \  RW, TMR interrupt enable
$04 constant R32_TMR_STATUS      \  RW, TMR status
$06 constant R8_TMR_INT_FLAG     \  RW1, TMR interrupt flag
$07 constant R8_TMR_FIFO_COUNT   \  RO, TMR FIFO count status
$08 constant R32_TMR_COUNT       \  RO, TMR current count
$0C constant R32_TMR_CNT_END     \  RW, TMR end count value, only low 26 bit
$10 constant R32_TMR_FIFO        \  RO/WO, TMR FIFO register, only low 26 bit

$01 constant R8_TMR_CTRL_DMA     \ RW, TMR DMA control         ( only available for TMR1 and TMR2 )
$18 constant R32_TMR1_DMA_NOW    \ RW, TMR DMA current address ( only available for TMR1 and TMR2 )
$18 constant R32_TMR1_DMA_BEG    \ RW, TMR DMA begin address   ( only available for TMR1 and TMR2 )
$1C constant R32_TMR1_DMA_END    \ RW, TMR DMA end address     ( only available for TMR1 and TMR2 )

\ register flags/values
$8  constant  TMR_FIFO_SIZE             \  timer FIFO size (depth)
$0  constant  TMR_CTRL_MOD
$01 constant  RB_TMR_MODE_IN            \  RW, timer in mode: 0=timer/PWM, 1=capture/count
$02 constant  RB_TMR_ALL_CLEAR          \  RW, force clear timer FIFO and count
$04 constant  RB_TMR_COUNT_EN           \  RW, timer count enable
$08 constant  RB_TMR_OUT_EN             \  RW, timer output enable
$10 constant  RB_TMR_OUT_POLAR          \  RW, timer PWM output polarity: 0=default low and high action, 1=default high and low action
$10 constant  RB_TMR_CAP_COUNT          \  RW, count sub-mode if RB_TMR_MODE_IN=1: 0=capture, 1=count
$C0 constant  RB_TMR_PWM_REPEAT         \  RW, timer PWM repeat mode: 00=1, 01=4, 10=8, 11-16
$C0 constant  RB_TMR_CAP_EDGE           \  RW, timer capture edge mode: 00=disable, 01=edge change, 10=fall to fall, 11-rise to rise

\ DMA flags
$01 constant  RB_TMR_DMA_ENABLE         \  RW, timer1/2 DMA enable
$04 constant  RB_TMR_DMA_LOOP           \  RW, timer1/2 DMA address loop enable

\ interrupt flags
$01 constant  RB_TMR_IE_CYC_END         \  RW, enable interrupt for timer capture count timeout or PWM cycle end
$02 constant  RB_TMR_IE_DATA_ACT        \  RW, enable interrupt for timer capture input action or PWM trigger
$04 constant  RB_TMR_IE_FIFO_HF         \  RW, enable interrupt for timer FIFO half (capture fifo >=4 or PWM fifo <=3)
$08 constant  RB_TMR_IE_DMA_END         \  RW, enable interrupt for timer1/2 DMA completion
$10 constant  RB_TMR_IE_FIFO_OV         \  RW, enable interrupt for timer FIFO overflow
$01 constant  RB_TMR_IF_CYC_END         \  RW1, interrupt flag for timer capture count timeout or PWM cycle end
$02 constant  RB_TMR_IF_DATA_ACT        \  RW1, interrupt flag for timer capture input action or PWM trigger
$04 constant  RB_TMR_IF_FIFO_HF         \  RW1, interrupt flag for timer FIFO half (capture fifo >=4 or PWM fifo <=3)
$08 constant  RB_TMR_IF_DMA_END         \  RW1, interrupt flag for timer1/2 DMA completion
$10 constant  RB_TMR_IF_FIFO_OV         \  RW1, interrupt flag for timer FIFO overflow

: timer-set  ( endvalue base-addr )
  R32_TMR_CNT_END + ! ( base-addr )
;

: timer-init ( endvalue base-addr )
  swap over R32_TMR_CNT_END + ! ( base-addr )
  R8_TMR_CTRL_MOD +             ( R8_TMR_CTRL_MOD )
  dup RB_TMR_ALL_CLEAR swap c!
  RB_TMR_COUNT_EN RB_TMR_CAP_COUNT or swap c!
;

: timer-dma-config ( startAddr endAddr loop? base-addr )
  dup TMR0 = if
    ." DMA is not available for TMR0 "
    2drop 2drop quit
  then

  >r ( startAddr endAddr )  \ tuck away timer base address
  RB_TMR_DMA_ENABLE
  swap
  if \ loop?
    RB_TMR_DMA_LOOP or
  then  ( startAddr endAddr config )
  -rot  ( config startAddr endAddr )
  r@ R32_TMR1_DMA_END + !
  r@ R32_TMR1_DMA_BEG + !
  r> R8_TMR_CTRL_DMA  + c!
;

: timer-enable-interrupt ( base-addr )
  R8_TMR_INTER_EN +
  dup c@                  ( reg-addr regval )
  RB_TMR_IE_CYC_END or
  swap c!
;

: tmr-get-intflag  ( tmrbase flag -- flagval )
  swap R8_TMR_INT_FLAG + c@ ( flag intflags )
  and
;

: tmr-clear-intflag  ( tmrbase flag )
  swap               ( flag tmrbase )
  R8_TMR_INT_FLAG + c!
;

: tmr-current ( tmrbase -- count )
  R32_TMR_COUNT + @
;