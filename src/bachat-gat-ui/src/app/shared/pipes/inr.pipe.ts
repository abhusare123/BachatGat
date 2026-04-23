// INR pipe — formats numbers with Indian lakh/crore grouping, no decimals
// e.g. 248000 → "2,48,000"  ·  15000000 → "1,50,00,000"

import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'inr',
  standalone: true,
})
export class InrPipe implements PipeTransform {
  transform(value: number | null | undefined, withSymbol: boolean = false): string {
    if (value == null || Number.isNaN(value)) return '';

    const rounded = Math.round(value);
    const formatted = new Intl.NumberFormat('en-IN', {
      maximumFractionDigits: 0,
      minimumFractionDigits: 0,
    }).format(rounded);

    return withSymbol ? `₹${formatted}` : formatted;
  }
}
