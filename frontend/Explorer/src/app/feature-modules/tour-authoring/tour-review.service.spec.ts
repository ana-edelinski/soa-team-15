import { TestBed } from '@angular/core/testing';

import { TourReviewService } from './tour-review.service';

describe('TourReviewService', () => {
  let service: TourReviewService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TourReviewService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
