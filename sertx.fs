#require serdes.fs

SDS_PLL_FREQ_300M serdes-tx-init
1024 constant DMA_SIZE_WORDS
create dma-buffer DMA_SIZE_WORDS cells allot

: dma-buffer-init
  DMA_SIZE_WORDS 0 do
    dup
    i 2 lshift dma-buffer + !
  loop
  ;

$aaaa5555 dma-buffer-init

: tx-loop
  begin
    dma-buffer DMA_SIZE_WORDS 2 lshift $0fffffff serdes-dma-tx-config
    [char] . emit
  key? until
;