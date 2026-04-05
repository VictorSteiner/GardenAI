import type { DetailedHTMLProps, HTMLAttributes } from 'react'

declare module 'react' {
  namespace JSX {
    interface IntrinsicElements {
      'ha-card': DetailedHTMLProps<HTMLAttributes<HTMLElement>, HTMLElement>
    }
  }
}

export {}

