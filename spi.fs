#require gpio.fs

\ SPI register base
$40004000 constant SPI0
$40004400 constant SPI1

\ SPI register offsets
$00 constant R8_CTRL_MOD          \ RW, mode control
$01 constant R8_CTRL_CFG          \ RW, configuration control
$02 constant R8_INTER_EN          \ RW, interrupt enable
$03 constant R8_CLOCK_DIV         \ RW, master clock divisor
$03 constant R8_SLAVE_PRE         \ RW, slave preset value
$04 constant R32_STATUS           \ RW, status
$04 constant R8_BUFFER            \ RO, data buffer
$05 constant R8_RUN_FLAG          \ RO, work flag
$06 constant R8_INT_FLAG          \ RW1, interrupt flag
$07 constant R8_FIFO_COUNT        \ RO, FIFO count status
$0C constant R32_TOTAL_CNT        \ RW, total byte count, only low 12 bit
$0C constant R16_TOTAL_CNT        \ RW, total byte count, only low 12 bit
$10 constant R32_FIFO             \ RW, FIFO register
$10 constant R8_FIFO              \ RO/WO, FIFO register
$13 constant R8_FIFO_COUNT1       \ RO, FIFO count status
$14 constant R32_DMA_NOW          \ RW, DMA current address
$18 constant R32_DMA_BEG          \ RW, DMA begin address
$1C constant R32_DMA_END          \ RW, DMA end address

\ configuration bits
$01 constant SPI_MODE_SLAVE    \ RW, SPI slave mode: 0=master/host, 1=slave/device
$02 constant SPI_ALL_CLEAR     \ RW, force clear SPI FIFO and count
$04 constant SPI_2WIRE_MOD     \ RW, SPI enable 2 wire mode: 0=3wire(SCK,MOSI,MISO), 1=2wire(SCK,MISO=MXSX)
$08 constant SPI_MST_SCK_MOD   \ RW, SPI master clock mode: 0=mode 0, 1=mode 3
$08 constant SPI_SLV_CMD_MOD   \ RW, SPI slave command mode: 0=byte stream, 1=first byte command
$10 constant SPI_FIFO_DIR      \ RW, SPI FIFO direction: 0=out(write @master mode), 1=in(read @master mode)
$20 constant SPI_SCK_OE        \ RW, SPI SCK output enable
$40 constant SPI_MOSI_OE       \ RW, SPI MOSI output enable
$40 constant SPI1_SDO_OE       \ RW, SPI1 SDO output enable
$80 constant SPI_MISO_OE       \ RW, SPI MISO output enable
$80 constant SPI1_SDI_OE       \ RW, SPI1 SDI output enable, SPI1 enable 2 wire mode: 0=3wire(SCK1,SDO,SDI), 1=2wire(SCK1,SDI=SDX)
$01 constant SPI_DMA_ENABLE    \ RW, SPI DMA enable
$04 constant SPI_DMA_LOOP      \ RW, SPI DMA address loop enable
$10 constant SPI_AUTO_IF       \ RW, enable buffer/FIFO accessing to auto clear RB_SPI_IF_BYTE_END interrupt flag
$20 constant SPI_BIT_ORDER     \ RW, SPI bit data order: 0=MSB first, 1=LSB first
$01 constant SPI_IE_CNT_END    \ RW, enable interrupt for SPI total byte count end
$02 constant SPI_IE_BYTE_END   \ RW, enable interrupt for SPI byte exchanged
$04 constant SPI_IE_FIFO_HF    \ RW, enable interrupt for SPI FIFO half
$08 constant SPI_IE_DMA_END    \ RW, enable interrupt for SPI DMA completion
$10 constant SPI_IE_FIFO_OV    \ RW, enable interrupt for SPI FIFO overflow
$80 constant SPI_IE_FST_BYTE   \ RW, enable interrupt for SPI slave mode first byte received
$10 constant SPI_SLV_CMD_ACT   \ RO, SPI slave command flag
$20 constant SPI_FIFO_READY    \ RO, SPI FIFO ready status
$40 constant SPI_SLV_CS_LOAD   \ RO, SPI slave chip-select loading status
$80 constant SPI_SLV_SELECT    \ RO, SPI slave selection status
$01 constant SPI_IF_CNT_END    \ RW1, interrupt flag for SPI total byte count end
$02 constant SPI_IF_BYTE_END   \ RW1, interrupt flag for SPI byte exchanged
$04 constant SPI_IF_FIFO_HF    \ RW1, interrupt flag for SPI FIFO half (RB_SPI_FIFO_DIR ? >=4bytes : <4bytes)
$08 constant SPI_IF_DMA_END    \ RW1, interrupt flag for SPI DMA completion
$10 constant SPI_IF_FIFO_OV    \ RW1, interrupt flag for SPI FIFO overflow
$40 constant SPI_FREE          \ RO, current SPI free status
$80 constant SPI_IF_FST_BYTE   \ RW1, interrupt flag for SPI slave mode first byte received
$0C constant SPI_TOTAL_CNT
$10 constant SPI_FIFO
$14 constant SPI_DMA_NOW
$18 constant SPI_DMA_BEG
$1C constant SPI_DMA_END

1 constant SPI_CTRL_CFG
2 constant SPI_INTER_EN
3 constant SPI_CLOCK_DIV
3 constant SPI_SLAVE_PRESET
4 constant SPI_BUFFER
5 constant SPI_RUN_FLAG
6 constant SPI_INT_FLAG
7 constant SPI_FIFO_COUNT

: spi0-cs-low ( -- )
    [ pin 12 literal, ] PortA CLEAR gpio-set
;

: spi0-cs-high ( -- )
    [ pin 12 literal, ] PortA OUT gpio-set
;

: spi-config-write ( spi-base conf-reg value -- )
   -rot ( value spi-base conf-reg )
   +    ( value conf-reg-addr )
   c!
;

: spi-config-set ( spi-base conf-reg value -- )
   -rot ( value spi-base conf-reg )
   +    ( value conf-reg-addr )
   dup  ( value conf-reg-addr conf-reg-addr )
   c@   ( value conf-reg-addr reg-val )
   rot  ( conf-reg-addr reg-val value )
   or   ( conf-reg-addr new-reg-val )
   swap
   c!
;

: spi-config-clear ( spi-base conf-reg value -- )
   -rot ( value spi-base conf-reg )
   +    ( value conf-reg-addr )
   dup  ( value conf-reg-addr conf-reg-addr )
   c@   ( value conf-reg-addr reg-val )
   rot  ( conf-reg-addr reg-val value )
   not  ( conf-reg-addr reg-val ~value )
   and  ( conf-reg-addr new-reg-val )
   swap
   c!
;

: spi-master-init ( spi-base -- )
  dup R8_CLOCK_DIV $14                       spi-config-write
  dup R8_CTRL_MOD  SPI_ALL_CLEAR             spi-config-write
  dup R8_CTRL_MOD  SPI_MOSI_OE SPI_SCK_OE or spi-config-write

  [ pin 12 pin 13 pin 14 or or literal, ]
  PortA DIRECTION +
  bis!

  [ pin 12 literal, ] PortA PULLUP gpio-set

  dup R8_CTRL_CFG SPI_AUTO_IF spi-config-set
  R8_CTRL_CFG SPI_DMA_ENABLE spi-config-clear
;

: spi-wait-fifo ( spi-base -- )
  R8_INT_FLAG +
  begin dup c@ SPI_FREE and 0<> until
  drop
;

: spi-tx ( c spi-base -- )
  dup R8_CTRL_MOD SPI_FIFO_DIR spi-config-clear

  tuck           ( spi-base c spi-base )
  R8_BUFFER + c! ( spi-base )

  spi-wait-fifo
;

: spi-rx ( spi-base -- data )
  dup R8_CTRL_MOD SPI_FIFO_DIR spi-config-clear
  dup R8_BUFFER   $ff          spi-config-write

  dup spi-wait-fifo

  R8_BUFFER + c@
;

: spi-fifo-count ( spi-base -- count )
  R8_FIFO_COUNT + c@
;

: spi-fifo-byte ( spi-base -- databyte )
  R8_FIFO + c@
;

\ this allots a buffer of the given size and returns
\ the pointer to the buffer
\ it is the responsibility of the user to deallocate it
\ after its use
: spi-rx-many ( spi-base nbytes -- data-addr )
  here dup >r >r    \ save buffer location
  dup allot         \ allocate buffer
  ( spi-base nbytes )
  over R8_CTRL_MOD SPI_FIFO_DIR spi-config-set
  over R16_TOTAL_CNT + over swap h!
  over R8_INT_FLAG + SPI_IF_CNT_END swap c!
  ( spi-base nbytes )
  begin
    over spi-fifo-count 0 u> if
      ( spi-base nbytes )
      over spi-fifo-byte \ dup hex . space decimal
      ( spi-base nbytes data )
      \ write next byte addr back to return stack, keep this byte
      r> dup 1+ >r
      ( spi-base nbytes data destaddr )
      c!
      ( spi-base nbytes )
      1-
    then
  dup 0= until
  drop drop r> drop
  r>
;