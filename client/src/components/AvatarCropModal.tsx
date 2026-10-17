import { useEffect, useRef, useState, type PointerEvent as ReactPointerEvent } from 'react'
import { useTranslation } from 'react-i18next'

type Props = {
  file: File
  onCancel: () => void
  onConfirm: (file: File) => void
}

const FRAME = 260
const OUTPUT = 320

export function AvatarCropModal({ file, onCancel, onConfirm }: Props) {
  const { t } = useTranslation()
  const imgRef = useRef<HTMLImageElement | null>(null)
  const [objectUrl, setObjectUrl] = useState<string | null>(null)
  const [natural, setNatural] = useState({ w: 0, h: 0 })
  const [scale, setScale] = useState(1)
  const [minScale, setMinScale] = useState(1)
  const [offset, setOffset] = useState({ x: 0, y: 0 })
  const [dragging, setDragging] = useState(false)
  const dragStart = useRef({ x: 0, y: 0, ox: 0, oy: 0 })
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    const url = URL.createObjectURL(file)
    setObjectUrl(url)
    return () => URL.revokeObjectURL(url)
  }, [file])

  function onImageLoad(img: HTMLImageElement) {
    const w = img.naturalWidth
    const h = img.naturalHeight
    setNatural({ w, h })
    const cover = Math.max(FRAME / w, FRAME / h)
    setMinScale(cover)
    setScale(cover)
    setOffset({ x: (FRAME - w * cover) / 2, y: (FRAME - h * cover) / 2 })
  }

  function clampOffset(next: { x: number; y: number }, nextScale: number) {
    const dw = natural.w * nextScale
    const dh = natural.h * nextScale
    const minX = Math.min(0, FRAME - dw)
    const minY = Math.min(0, FRAME - dh)
    const maxX = Math.max(0, FRAME - dw)
    const maxY = Math.max(0, FRAME - dh)
    return {
      x: Math.min(maxX, Math.max(minX, next.x)),
      y: Math.min(maxY, Math.max(minY, next.y)),
    }
  }

  function centerImage(nextScale = scale) {
    const dw = natural.w * nextScale
    const dh = natural.h * nextScale
    setOffset(clampOffset({ x: (FRAME - dw) / 2, y: (FRAME - dh) / 2 }, nextScale))
  }

  function onPointerDown(e: ReactPointerEvent<HTMLDivElement>) {
    e.currentTarget.setPointerCapture(e.pointerId)
    setDragging(true)
    dragStart.current = { x: e.clientX, y: e.clientY, ox: offset.x, oy: offset.y }
  }

  function onPointerMove(e: ReactPointerEvent<HTMLDivElement>) {
    if (!dragging) return
    const dx = e.clientX - dragStart.current.x
    const dy = e.clientY - dragStart.current.y
    setOffset(
      clampOffset(
        { x: dragStart.current.ox + dx, y: dragStart.current.oy + dy },
        scale,
      ),
    )
  }

  function onPointerUp(e: ReactPointerEvent<HTMLDivElement>) {
    e.currentTarget.releasePointerCapture(e.pointerId)
    setDragging(false)
  }

  function onScaleChange(value: number) {
    const next = Math.max(minScale, value)
    const cx = FRAME / 2
    const cy = FRAME / 2
    const relX = (cx - offset.x) / scale
    const relY = (cy - offset.y) / scale
    const nextOffset = clampOffset(
      { x: cx - relX * next, y: cy - relY * next },
      next,
    )
    setScale(next)
    setOffset(nextOffset)
  }

  async function exportCropped() {
    if (!natural.w || !natural.h || !objectUrl) return
    setBusy(true)
    try {
      const image = imgRef.current
      if (!image || !image.complete || image.naturalWidth === 0) return

      const canvas = document.createElement('canvas')
      canvas.width = OUTPUT
      canvas.height = OUTPUT
      const ctx = canvas.getContext('2d')
      if (!ctx) return

      // Opaque white under JPEG (avoids black/empty look outside drawn pixels)
      ctx.fillStyle = '#ffffff'
      ctx.fillRect(0, 0, OUTPUT, OUTPUT)

      ctx.save()
      ctx.beginPath()
      ctx.arc(OUTPUT / 2, OUTPUT / 2, OUTPUT / 2, 0, Math.PI * 2)
      ctx.closePath()
      ctx.clip()

      const ratio = OUTPUT / FRAME
      // Map frame viewport → source pixels, then draw into full output square
      const sx = Math.max(0, -offset.x / scale)
      const sy = Math.max(0, -offset.y / scale)
      const sw = Math.min(natural.w - sx, FRAME / scale)
      const sh = Math.min(natural.h - sy, FRAME / scale)
      const dx = Math.max(0, offset.x) * ratio
      const dy = Math.max(0, offset.y) * ratio
      const dw = sw * scale * ratio
      const dh = sh * scale * ratio

      ctx.drawImage(image, sx, sy, sw, sh, dx, dy, dw, dh)
      ctx.restore()

      const blob = await new Promise<Blob | null>((resolve) =>
        canvas.toBlob(resolve, 'image/jpeg', 0.92),
      )
      if (!blob || blob.size < 100) return

      const base = file.name.replace(/\.[^.]+$/, '') || 'avatar'
      onConfirm(new File([blob], `${base}.jpg`, { type: 'image/jpeg' }))
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="avatar-crop-overlay" role="dialog" aria-modal="true">
      <div className="avatar-crop-panel">
        <h2>{t('profile.cropTitle')}</h2>
        <p className="muted">{t('profile.cropHint')}</p>

        <div
          className="avatar-crop-frame"
          style={{ width: FRAME, height: FRAME, cursor: dragging ? 'grabbing' : 'grab' }}
          onPointerDown={onPointerDown}
          onPointerMove={onPointerMove}
          onPointerUp={onPointerUp}
          onPointerCancel={onPointerUp}
        >
          {objectUrl && (
            <img
              ref={imgRef}
              src={objectUrl}
              alt=""
              draggable={false}
              onLoad={(e) => onImageLoad(e.currentTarget)}
              style={{
                position: 'absolute',
                left: offset.x,
                top: offset.y,
                width: natural.w ? natural.w * scale : 'auto',
                height: natural.h ? natural.h * scale : 'auto',
                maxWidth: 'none',
                userSelect: 'none',
                pointerEvents: 'none',
              }}
            />
          )}
        </div>

        <label className="avatar-crop-zoom">
          {t('profile.cropZoom')}
          <input
            type="range"
            min={minScale}
            max={Math.max(minScale * 3, minScale + 0.01)}
            step={0.01}
            value={scale}
            onChange={(e) => onScaleChange(Number(e.target.value))}
          />
        </label>

        <div className="actions-cell">
          <button type="button" className="btn ghost" onClick={() => centerImage()} disabled={busy}>
            {t('profile.cropCenter')}
          </button>
          <button type="button" className="btn ghost" onClick={onCancel} disabled={busy}>
            {t('common.cancel')}
          </button>
          <button type="button" className="btn success" onClick={() => void exportCropped()} disabled={busy}>
            {busy ? t('common.loading') : t('profile.cropApply')}
          </button>
        </div>
      </div>
    </div>
  )
}
