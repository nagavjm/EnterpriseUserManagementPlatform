import { TestBed } from '@angular/core/testing';
import { ConfirmationDialogService } from './confirmation-dialog.service';

describe('ConfirmationDialogService', () => {
  let service: ConfirmationDialogService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ConfirmationDialogService]
    });
    service = TestBed.inject(ConfirmationDialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should emit open state when confirm() is invoked', (done) => {
    service.dialogState$.subscribe(state => {
      if (state.isOpen) {
        expect(state.options.title).toBe('Unsaved Changes');
        expect(state.options.message).toBe('Leave page?');
        expect(state.options.confirmButtonText).toBe('Discard');
        done();
      }
    });

    service.confirm({
      title: 'Unsaved Changes',
      message: 'Leave page?',
      confirmButtonText: 'Discard'
    });
  });

  it('should resolve true when confirmAction() is called', (done) => {
    service.confirm({
      message: 'Are you sure?'
    }).subscribe(result => {
      expect(result).toBeTrue();
      done();
    });

    service.confirmAction();
  });

  it('should resolve false when cancelAction() is called', (done) => {
    service.confirm({
      message: 'Are you sure?'
    }).subscribe(result => {
      expect(result).toBeFalse();
      done();
    });

    service.cancelAction();
  });
});
