\ internal SPI Flash register
$40001014 constant R32_ROM_ADDR
$40001018 constant R32_ROM_CTRL
$40001018 constant R8_ROM_DATA   \ RW
$4000101A constant R16_ROM_CR    \ RW

$8000 constant ROM_ADDR_OFFSET

\ internal SPI Flash register address offset and bit defines
0 constant ROM_DATA
1 constant ROM_CR
1 constant ROM_SCS
2 constant ROM_SIO0_OE
4 constant ROM_RD_EN
8 constant ROM_WR8
0 constant ROM_DES_LONG

: rom-data-write ( databyte -- )
  begin
    R16_ROM_CR c@  \ get status byte
    24 lshift
    24 arshift
  0 >= until
  R8_ROM_DATA c!
;

: rom-data-read ( -- databyte )
  begin
    R16_ROM_CR c@  \ get status byte
    24 lshift
    24 arshift
  0 >= until
  R8_ROM_DATA c@
;

: rom-access-end ( -- )
  begin
    R16_ROM_CR c@  \ get status byte
    24 lshift
    24 arshift
  0 >= until
  0 R16_ROM_CR c!
;

: rom-begin-read ( -- )
  0     R16_ROM_CR   c!
  %111  R16_ROM_CR   c!
  %1011 R32_ROM_CTRL c!
;

: rom-write-addr ( addr -- )
  dup 16 rshift $ff and rom-data-write
  dup 8  rshift $ff and rom-data-write
                $ff and rom-data-write
;

: rom-read-byte-internal ( -- databyte )
    rom-data-read drop
    rom-data-read drop
    rom-data-read drop
    R8_ROM_DATA c@
;

: rom-read-byte ( addr -- )
  ROM_ADDR_OFFSET +
  rom-begin-read
  rom-write-addr
  rom-read-byte-internal
  rom-access-end
;

: rom-read-word ( addr -- word )
  ROM_ADDR_OFFSET +
  rom-begin-read
  rom-write-addr
  rom-data-read drop rom-data-read drop
  rom-read-byte-internal
  rom-read-byte-internal  8 lshift or
  rom-read-byte-internal 16 lshift or
  rom-read-byte-internal 24 lshift or
  rom-access-end
;