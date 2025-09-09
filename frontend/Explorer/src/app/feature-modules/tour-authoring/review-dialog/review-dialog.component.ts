import { Component, Inject, ElementRef, ViewChild } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { User } from 'src/app/infrastructure/auth/model/user.model';
import { TourReviewService } from '../tour-review.service';

type DialogData = { tourId: number; user: User };

@Component({
  selector: 'xp-review-dialog',
  templateUrl: './review-dialog.component.html',
  styleUrls: ['./review-dialog.component.css']
})
export class ReviewDialogComponent {
  loading = false;
  error = '';

  // 👉 default date = danas, pa Submit nije sivo kad sve popuniš
  form = this.fb.group({
    rating: [5, [Validators.required, Validators.min(1), Validators.max(5)]],
    comment: [''],
    visited: [new Date(), [Validators.required]]   // 👈 default
  });

  // upload state
  selectedFiles: File[] = [];
  previews: string[] = [];
  selectedIndex = 0;

  // da “simuliramo” klik na file input
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  constructor(
    private fb: FormBuilder,
    private reviewService: TourReviewService,
    private dialogRef: MatDialogRef<ReviewDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData
  ) {}

  openFileDialog() {
    this.fileInput?.nativeElement.click();
  }

  onFilesSelected(evt: Event) {
    const input = evt.target as HTMLInputElement;
    if (!input.files) return;

    const incoming = Array.from(input.files);

    // APPEND režim (ne briši stare); izbegni duplikate po name+size+mtime
    for (const f of incoming) {
      const dup = this.selectedFiles.find(
        x => x.name === f.name && x.size === f.size && x.lastModified === f.lastModified
      );
      if (dup) continue;

      this.selectedFiles.push(f);

      const reader = new FileReader();
      reader.onload = () => this.previews.push(reader.result as string);
      reader.readAsDataURL(f);
    }

    // reset value da bi “change” radio i za iste fajlove ponovo
    input.value = '';
    // ako je prva slika dodata, pokaži je
    if (this.previews.length && this.selectedIndex >= this.previews.length) {
      this.selectedIndex = this.previews.length - 1;
    }
  }

  removeAt(i: number) {
    if (i < 0 || i >= this.previews.length) return;
    this.previews.splice(i, 1);
    this.selectedFiles.splice(i, 1);
    if (this.selectedIndex >= this.previews.length) {
      this.selectedIndex = Math.max(0, this.previews.length - 1);
    }
  }

  clearAll() {
    this.previews = [];
    this.selectedFiles = [];
    this.selectedIndex = 0;
  }

  prev() { if (this.previews.length) this.selectedIndex = (this.selectedIndex - 1 + this.previews.length) % this.previews.length; }
  next() { if (this.previews.length) this.selectedIndex = (this.selectedIndex + 1) % this.previews.length; }
  select(i: number) { this.selectedIndex = i; }

  submit(): void {
    this.error = '';
    if (this.form.invalid || !this.data?.user) return;

    const v = this.form.value.visited!;
    const visitedIso = new Date(
      Date.UTC(v.getFullYear(), v.getMonth(), v.getDate(), 0, 0, 0)
    ).toISOString();

    const finalize = (imageUrls: string[]) => {
      const body = {
        idTourist: this.data.user.id,
        rating: this.form.value.rating!,
        comment: this.form.value.comment ?? '',
        dateTour: visitedIso,
        images: imageUrls          // 👈 više URL-ova
      };

      this.reviewService.create(this.data.tourId, body).subscribe({
        next: (created) => { this.loading = false; this.dialogRef.close(created); },
        error: (e) => { this.loading = false; this.error = e?.error ?? 'Failed to submit review.'; }
      });
    };

    this.loading = true;

    if (this.selectedFiles.length > 0) {
      this.reviewService.uploadImages(this.selectedFiles).subscribe({
        next: urls => finalize(urls),
        error: e => { this.loading = false; this.error = e?.error ?? 'Upload failed.'; }
      });
    } else {
      finalize([]);
    }
  }
}
