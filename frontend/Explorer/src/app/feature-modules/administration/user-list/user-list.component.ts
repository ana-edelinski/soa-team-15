import { Component } from '@angular/core';
import { Account } from 'src/app/infrastructure/auth/model/account.model';
import { AdministrationService } from '../administration.service';

@Component({
  selector: 'xp-user-list',
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.css']
})
export class UserListComponent {
  accounts: Account[] = [];
  filteredAccounts: Account[] = [];

  searchQuery: string = '';
  selectedRole: string = '';
  isChatOpen = false;
  chatMessage = 'Use filters to find accounts. Click to deactivate or deposit.';

  constructor(private adminService: AdministrationService) {}

  ngOnInit(): void {
    this.loadAccounts();
  }

  loadAccounts(): void {
    this.adminService.getAllAccounts().subscribe((data: any) => {
      const nonAdmins = data.results.filter((ac: any) => ac.role?.toLowerCase() !== 'administrator');
      console.log(nonAdmins)
      this.accounts = nonAdmins;
      this.filteredAccounts = nonAdmins;

    });
  }

  filterAccounts(): void {
    this.filteredAccounts = this.accounts.filter(ac =>
      (this.searchQuery ? (ac.username.toLowerCase().includes(this.searchQuery.toLowerCase()) || ac.email.toLowerCase().includes(this.searchQuery.toLowerCase())) : true) &&
      (this.selectedRole ? ac.role.toLowerCase() === this.selectedRole.toLowerCase() : true)
    );
  }

  

  onBlockClicked(user: Account) {
    this.adminService.blockUser(user).subscribe(() => {
      user.isActive = false;
    });
  }

  

}
